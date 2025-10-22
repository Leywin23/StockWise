using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using StockWise.Data;
using StockWise.Dtos.AccountDtos;
using StockWise.Extensions;
using StockWise.Helpers;
using StockWise.Interfaces;
using StockWise.Models;
using StockWise.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace StockWise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly StockWiseDb _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailSenderServicer _emailSenderServicer;
        private readonly IMemoryCache _cache;
        private readonly IAccountService _accountService;
        public AccountController(ITokenService tokenService, StockWiseDb context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailSenderServicer emailSenderServicer, IMemoryCache cache, IAccountService accountService)
        {
            _tokenService = tokenService;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSenderServicer = emailSenderServicer;
            _cache = cache;
            _accountService = accountService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _accountService.RegisterAsync(userDto);

            return this.ToActionResult(result);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDataDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiError.From(new Exception(),StatusCodes.Status400BadRequest, HttpContext));
            }
            var result = await _accountService.LoginAsync(loginDto);

            return this.ToActionResult(result);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            var result = await _accountService.LogoutAsync(ct,this);
            return this.ToActionResult(result);
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(string email, string code)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiError.From(new Exception(), StatusCodes.Status400BadRequest, HttpContext));
            }
            var result = await _accountService.VerifyEmailAsync(email, code);
            return this.ToActionResult(result);
        }

        [HttpPost("send-reset-password")]
        public async Task<IActionResult> SendRequestToRestartPassword(string email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiError.From(new Exception(), StatusCodes.Status400BadRequest, HttpContext));
            }
            var result = await _accountService.SendRequestToRestartPasswordAsync(email);
            return this.ToActionResult(result);
        }

        private static string GetResetKey(string email) => $"pwdreset:{email}";


        [HttpPost("Restart-Password")]
        public async Task<IActionResult> RestartPassword(string email, string code, string newPassword)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiError.From(new Exception(), StatusCodes.Status400BadRequest, HttpContext));
            }

            var result = await _accountService.RestartPasswordAsync(email, code, newPassword);
            return this.ToActionResult(result);
        }


        [Authorize]
        [HttpPost("approve-user/{userId}")]
        public async Task<IActionResult> ApproveUser(string userId)
        {
           var result = await _accountService.ApproveUserAsync(userId);
           return this.ToActionResult(result);
        }

        [Authorize(Roles = "Manager")]
        [HttpGet("companies/pending")]
        public async Task<IActionResult> GetAllPending()
        {
            var result = await _accountService.GetAllPendingAsync();
            return this.ToActionResult(result);
        }

        [HttpPost("companies/suspend/{userId}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> SuspendUserFromCompany(string userId)
        {
            var result = await _accountService.SuspendUserFromCompanyAsync(userId);
            return this.ToActionResult(result);
        }

        [HttpPost("companies/unsuspend/{userId}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> UnsuspendUserFromCompany(string userId)
        {
            var result = await _accountService.UnsuspendUserFromCompanyAsync(userId);
            return this.ToActionResult(result);
        }

        [HttpPost("companies/reject/{userId}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> RejectUserFromCompany(string userId)
        {
            var result = await _accountService.RejectUserFromCompanyAsync(userId);
            return this.ToActionResult(result);
        }

        [HttpPost("companies/change/{companyNIP}")]
        [Authorize(Roles = "Worker")]
        public async Task<IActionResult> ChangeUserCompany([FromRoute] string companyNIP)
        {
            var result = await _accountService.ChangeUserCompanyAsync(companyNIP);
            return this.ToActionResult(result);
        }

        [HttpPost("CompanyWithUser")]
        public async Task<IActionResult> CreateCompanyWithAccount(CreateCompanyWithAccountDto dto)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ApiError.From(new Exception("Bad Request"), StatusCodes.Status400BadRequest, HttpContext));
            }
            var result = await _accountService.CreateCompanyWithAccountAsync(dto);
            return this.ToActionResult(result);
        }

    }
}
