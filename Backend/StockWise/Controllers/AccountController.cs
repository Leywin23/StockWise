using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private static Dictionary<string, string> _verifyCodes = new Dictionary<string, string>();

        public AccountController(ITokenService tokenService, StockWiseDb context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailSenderServicer emailSenderServicer)
        {
            _tokenService = tokenService;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSenderServicer = emailSenderServicer;
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
                    IsEmailConfirmed = false,
                };

                var CreatedUser = await _userManager.CreateAsync(newUser, userDto.Password);

                if (CreatedUser.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(newUser, "Worker");
                    if (roleResult.Succeeded)
                    {
                        var VerifyCode = GenerateVerifyCode();
                        _verifyCodes[newUser.Email] = VerifyCode;

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
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
                if (user == null)
                {
                    return BadRequest("Email or Password is wrong");
                }
                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
                if (!result.Succeeded)
                {
                    return Unauthorized("Email not found and/or password incorrect");
                }
                return Ok(new NewUserDto
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

            if (user.IsEmailConfirmed)
                return BadRequest(new { message = "Email already verified." });

            if (!_verifyCodes.TryGetValue(email, out var storedCode))
                return BadRequest(new { message = "Verification code not found or expired." });

            user.IsEmailConfirmed = true;
            var update = await _userManager.UpdateAsync(user);
            if (!update.Succeeded)
                return StatusCode(500, update.Errors);

            _verifyCodes.Remove(email);

            return Ok(new { message = "Email verified." });

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
