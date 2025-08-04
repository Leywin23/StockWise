using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Dtos.CompanyDtos;
using StockWise.Models;

namespace StockWise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly StockWiseDb _context;

        public CompanyController(StockWiseDb context)
        {
            _context = context;
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
    }
}
