using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Dtos.CompanyDtos;
using StockWise.Models;
using System.Security.Claims;

namespace StockWise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly StockWiseDb _context;
        private readonly UserManager<AppUser> _userManager;

        public CompanyController(StockWiseDb context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetCompanyData(int id)
        {
            var userName = User.FindFirst(ClaimTypes.Name).Value;
            if (userName == null)
            {
                return Unauthorized();
            }
            var user = await _userManager.Users.Include(u => u.Company).FirstOrDefaultAsync(x => x.UserName == userName);

            var company = await _context.Companies.FirstOrDefaultAsync(c => c.NIP == user.Company.Id);
            if (company == null)
                return BadRequest("User is not assigned to any company.");

            return Ok(company);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCompany(CreateCompanyDto companyDto)
        {
            var companies = await _context.Companies
                .Where(c =>
                c.Name == companyDto.Name ||
                c.NIP == companyDto.NIP ||
                c.Address == companyDto.Address ||
                c.Email == companyDto.Email ||
                c.Phone == companyDto.Phone)
                .ToListAsync();

            var errors = new List<string>();

            foreach (var match in companies)
            {
                if (match.Name == companyDto.Name)
                {
                    errors.Add("A company with this name already exists");
                }

                if (match.NIP == companyDto.NIP)
                {
                    errors.Add("A company with this NIP already exists");
                }

                if (match.Address == companyDto.Address)
                {
                    errors.Add("A company with this address already exists");
                }
                if (match.Email == companyDto.Email)
                    errors.Add("A company with this email address already exists.");

                if (match.Phone == companyDto.Phone)
                    errors.Add("A company with this phone number already exists.");
            }
            if (errors.Any())
            {
                return BadRequest(new { errors });
            }

            var newCompany = new Company
            {
                Name = companyDto.Name,
                NIP = companyDto.NIP,
                Address = companyDto.Address,
                Email = companyDto.Email,
                Phone = companyDto.Phone,
            };

            await _context.Companies.AddAsync(newCompany);
            await _context.SaveChangesAsync();

            return Ok(newCompany);
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            var userName = User.FindFirst(ClaimTypes.Name).Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("Username not found in token");
            }
            var user = await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.UserName == userName);

            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == user.Company.Id);

            if (company == null)
                return BadRequest("User is not assigned to any company.");

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return Ok(company);
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> EditCompany(UpdateCompanyDto companyDto)
        {
            var userName = User.FindFirst(ClaimTypes.Name).Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("Username not found in token");
            }
            var user = await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.UserName == userName);

            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == user.Company.Id);

            if (company == null)
                return BadRequest("User is not assigned to any company.");

            company.Name = companyDto.Name;
            company.Email = companyDto.Email;
            company.Phone = companyDto.Phone;
            company.Address = companyDto.Address;

            _context.Companies.Update(company);
            await _context.SaveChangesAsync();

            return Ok(company);

        }
    }
}
