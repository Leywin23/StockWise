using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using StockWise.Data;
using StockWise.Dtos.AccountDtos;
using StockWise.Interfaces;
using StockWise.Models;

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
        public AccountController(ITokenService tokenService, StockWiseDb context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailSenderServicer emailSenderServicer, IMemoryCache cache)
        {
            _tokenService = tokenService;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSenderServicer = emailSenderServicer;
            _cache = cache;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto userDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userDto.Email);
                if (user != null)
                {
                    return BadRequest("User with this email already exist");
                }
                var comapany = await _context.Companies.FirstOrDefaultAsync(c => c.NIP == userDto.CompanyNIP);

                if (comapany == null) {
                    return NotFound($"There isn't any company with NIP: {userDto.CompanyNIP}");
                }

                var newUser = new AppUser
                {
                    Email = userDto.Email,
                    UserName = userDto.UserName,
                    Company = comapany,
                    EmailConfirmed = false,
                };

                var CreatedUser = await _userManager.CreateAsync(newUser, userDto.Password);

                if (CreatedUser.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(newUser, "Worker");
                    if (roleResult.Succeeded)
                    {
                        var VerifyCode = GenerateVerifyCode();
                        _cache.Set(newUser.Email, VerifyCode, TimeSpan.FromMinutes(10));

                        await _emailSenderServicer.SendEmailAsync(userDto.Email, "Verify Code", VerifyCode);
                        return Ok(new NewUserDto
                        {
                            UserName = userDto.UserName,
                            Email = userDto.Email,
                            CompanyName = comapany.Name,
                        });
                    }
                    else
                    {
                        return StatusCode(500, roleResult.Errors);
                    }
                }
                else
                {
                    return StatusCode(500, CreatedUser.Errors);
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "User creation failed.",
                    detail = ex.Message
                });
            }
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDataDto loginDto)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
                if (user == null)
                {
                    return BadRequest("Email or Password is wrong");
                }
                if (user.EmailConfirmed == false) return Unauthorized("Email is not confirmed");
                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
                if (!result.Succeeded)
                {
                    return Unauthorized("Email not found and/or password incorrect");
                }
                return Ok(new LoginDto
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    Token = await _tokenService.CreateToken(user)
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(string email, string code)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound("Email isn't assigned to any account");

            if (user.EmailConfirmed)
                return BadRequest(new { message = "Email already verified." });

            if (!_cache.TryGetValue(email, out string storedCode))
                return BadRequest(new { message = "Verification code not found or expired." });

            if(!string.Equals(storedCode, code, StringComparison.Ordinal))
                return BadRequest(new { message = "Verification code not found or expired." });

            user.EmailConfirmed = true;
            var update = await _userManager.UpdateAsync(user);
            if (!update.Succeeded)
                return StatusCode(500, update.Errors);

            _cache.Remove(email);

            return Ok(new { message = "Email verified." });

        }

        [HttpPost("send-reset-password")]
        public async Task<IActionResult> SendRequestToRestartPassword([FromBody] string email)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "Email is required." });

            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                var code = GenerateVerifyCode();

                _cache.Set(GetResetKey(email), code, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });

                await _emailSenderServicer.SendEmailAsync(email, "Reset password code", code);
            }

            return Ok(new { message = "If the email exists, a reset code was sent." });
        }

        private static string GetResetKey(string email) => $"pwdreset:{email}";


        [HttpPost("Restart-Password")]
        public async Task<IActionResult> RestartPassword(string email, string code, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return NotFound("Email isn't assigned to any account");

            var cacheKey = GetResetKey(email);
            if (!_cache.TryGetValue(cacheKey, out string storedCode))
                return BadRequest(new { message = "Verification code not found or expired." });

            if (!string.Equals(storedCode, code, StringComparison.Ordinal))
                return BadRequest(new { message = "Invalid verification code." });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded) return BadRequest(result.Errors);

            _cache.Remove(cacheKey);
            return Ok(new { message = "Password has been reset successfully." });
        }

        public static string GenerateVerifyCode(int length = 6)
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(
                Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)])
                    .ToArray()
            );
        }

        [Authorize(Roles ="Manager")]
        [HttpPost("approve-user/{userId}")]
        public async Task<IActionResult> ApproveUser(string userId)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser is null)
                return Unauthorized();

            if (currentUser.CompanyId is null)
                return BadRequest("You are not assigned to a company.");

            var userToApprove = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (userToApprove is null)
                return NotFound("User not found.");

            if (userToApprove.CompanyId != currentUser.CompanyId)
                return BadRequest("User does not belong to your company.");

            if (userToApprove.CompanyMembershipStatus != CompanyMembershipStatus.Pending)
                return BadRequest("User is not in Pending status.");

            userToApprove.CompanyMembershipStatus = CompanyMembershipStatus.Approved;

            var result = await _userManager.UpdateAsync(userToApprove);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { errors = result.Errors.Select(e => e.Description) });

            return Ok(new { message = $"User {userToApprove.UserName} approved" });
        }

        [Authorize(Roles = "Manager")]
        [HttpGet("companies/pending")]
        public async Task<IActionResult> GetAllPending()
        {
            var user = await GetCurrentUserAsync();
            if (user is null) return Unauthorized();
            if (user.CompanyId is null) return BadRequest("You are not assigned to a company.");

            var pendings = await _context.Users
                .Where(u => u.CompanyId == user.CompanyId && u.CompanyMembershipStatus == CompanyMembershipStatus.Pending)
                .ToListAsync();

            if (!pendings.Any()) return NotFound("There aren't any pending users.");

            return Ok(pendings);
        }

        private async Task<AppUser?> GetCurrentUserAsync()
        {
            var userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName)) return null;

            return await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.UserName == userName);
        }

        [HttpPost("companies/suspend/{userId}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> SuspendUserFromCompany(string userId)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser is null) return Unauthorized();
            var user = await _userManager.FindByIdAsync(userId);
            if (user.Id == currentUser.Id)
                return BadRequest("You cannot suspend yourself.");

            if (user is null) return NotFound("User not found");
            if (user.CompanyId != currentUser.CompanyId) return BadRequest("User isn't aligned to your company");
            if (user.CompanyMembershipStatus == CompanyMembershipStatus.Suspended)
                return BadRequest("User is already suspended.");

            user.CompanyMembershipStatus = CompanyMembershipStatus.Suspended;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return StatusCode(500, result.Errors);

            return Ok(new { message = $"User {user.UserName} has been suspended." });
        }

        [HttpPost("companies/unsuspend/{userId}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> UnsuspendUserFromCompany(string userId)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser is null) return Unauthorized();
            if (currentUser.CompanyId is null) return BadRequest("You are not assigned to a company.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return NotFound("User not found.");
            if (user.Id == currentUser.Id) return BadRequest("You cannot unsuspend yourself.");
            if (user.CompanyId != currentUser.CompanyId) return Forbid("User does not belong to your company.");
            if (user.CompanyMembershipStatus != CompanyMembershipStatus.Suspended)
                return BadRequest("User is not suspended.");

            user.CompanyMembershipStatus = CompanyMembershipStatus.Approved;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return StatusCode(StatusCodes.Status500InternalServerError, result.Errors);

            return Ok(new { message = $"User {user.UserName} has been unsuspended." });
        }

        [HttpPost("companies/reject/{userId}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> RejectUserFromCompany(string userId)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser is null) return Unauthorized();
            if (currentUser.CompanyId is null) return BadRequest("You are not assigned to a company.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return NotFound("User not found.");
            if (user.Id == currentUser.Id) return BadRequest("You cannot reject yourself.");
            if (user.CompanyId != currentUser.CompanyId) return Forbid("User does not belong to your company.");

            if (user.CompanyMembershipStatus == CompanyMembershipStatus.Rejected)
                return BadRequest("User is already rejected.");
            if (user.CompanyMembershipStatus != CompanyMembershipStatus.Pending)
                return BadRequest("Only users in Pending status can be rejected.");

            user.CompanyMembershipStatus = CompanyMembershipStatus.Rejected;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return StatusCode(StatusCodes.Status500InternalServerError, result.Errors);

            return Ok(new { message = $"User {user.UserName} has been rejected." });
        }

        [HttpPost("companies/change/{companyNIP}")]
        [Authorize(Roles = "Worker")]
        public async Task<IActionResult> ChangeUserCompany([FromRoute] long companyNIP)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser is null) return Unauthorized();

            var company = await _context.Companies.FirstOrDefaultAsync(c => c.NIP == companyNIP);
            if (company is null) return NotFound("Company with this NIP not found.");

            if (currentUser.CompanyId == company.Id)
                return BadRequest("You are already assigned to this company.");

            currentUser.CompanyId = company.Id;
            currentUser.Company = company;
            currentUser.CompanyMembershipStatus = CompanyMembershipStatus.Pending;

            var result = await _userManager.UpdateAsync(currentUser);
            if (!result.Succeeded) return StatusCode(StatusCodes.Status500InternalServerError, result.Errors);

            return Ok(new { message = $"User {currentUser.UserName} has changed company to {company.Name} (pending approval)." });
        }

    }
}
