using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Dtos;
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

        public AccountController(ITokenService tokenService, StockWiseDb context, UserManager<AppUser> userManager)
        {
            _tokenService = tokenService;
            _context = context;
            _userManager = userManager;
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

                var newUser = new AppUser
                {
                    Email = userDto.Email,
                    UserName = userDto.UserName,
                };

                var CreatedUser = await _userManager.CreateAsync(newUser, userDto.Password);

                if (CreatedUser.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(newUser, "User");
                    if (roleResult.Succeeded) {
                        return Ok(new NewUserDto
                        {
                            UserName = userDto.UserName,
                            Email = userDto.Email,
                            Token = _tokenService.CreateToken(newUser)
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
            catch (Exception ex) {
                return StatusCode(500, ex);
            }   
        }
    }
}
