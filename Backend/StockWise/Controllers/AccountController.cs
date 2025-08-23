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
                var comapany = await _context.Companies.FirstOrDefaultAsync(c=>c.NIP == userDto.CompanyNIP);

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
                    Token = _tokenService.CreateToken(user)
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

            if (!_cache.TryGetValue(email, out var storedCode))
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
        public async Task<IActionResult> RestartPassword(string email,string code, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return NotFound("Email isn't assigned to any account");

            if (!_cache.TryGetValue(email, out string storedCode)) return BadRequest(new { message = "Verification code not found or expired." });
            
            if(!string.Equals(storedCode, code, StringComparison.Ordinal))
                return BadRequest(new { message = "Invalid verification code." });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);
            _cache.Remove(email);

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
    }
}
