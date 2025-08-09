using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Dtos.CompanyDtos;
using StockWise.Interfaces;
using StockWise.Models;
using System.Security.Claims;

namespace StockWise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ICompanyService _companyService;
        private readonly StockWiseDb _context;

        public CompanyController(UserManager<AppUser> userManager, ICompanyService companyService, StockWiseDb context)
        {
            _userManager = userManager;
            _companyService = companyService;
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetCompanyData()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized("User not found");

            var company = await _companyService.GetCompanyAsync(user);
            if (company == null) return NotFound("Company not found");

            return Ok(company);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCompany(CreateCompanyDto companyDto)
        {
            var newCompany = await _companyService.CreateCompanyAsync(companyDto);
            if (newCompany == null)
                return BadRequest("Company already exists");

            return Ok(newCompany);
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteCompany()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized("User not found");

            var company = await _companyService.DeleteCompanyAsync(user);
            if (company == null)
                return NotFound("Company not found");

            return Ok(company);
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> EditCompany(UpdateCompanyDto companyDto)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized("User not found");

            var updatedCompany = await _companyService.UpdateCompanyAsync(companyDto, user);
            if (updatedCompany == null)
                return BadRequest("User is not assigned to any company.");

            return Ok(updatedCompany);
        }

        private async Task<AppUser?> GetCurrentUserAsync()
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
                return null;

            return await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.UserName == userName);
        }
    }
}
}
