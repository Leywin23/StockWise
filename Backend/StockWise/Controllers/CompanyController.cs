using AutoMapper;
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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace StockWise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ICompanyService _companyService;
        private readonly StockWiseDb _context;
        private readonly IMapper _mapper;

        public CompanyController(UserManager<AppUser> userManager, ICompanyService companyService, StockWiseDb context, IMapper mapper)
        {
            _userManager = userManager;
            _companyService = companyService;
            _context = context;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetCompanyData()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized("User not found");

            var company = await _companyService.GetCompanyAsync(user);
            if (company == null) return NotFound("Company not found");

            var companyDto = _mapper.Map<CompanyDto>(company);

            return Ok(companyDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCompany(CreateCompanyDto companyDto)
        {
            var newCompany = await _companyService.CreateCompanyAsync(companyDto);
            if (newCompany == null)
                return BadRequest("Company already exists");

            var newCompanyDto = _mapper.Map<CompanyDto>(newCompany);
            return Ok(newCompanyDto);
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

            var companyDto = _mapper.Map<CompanyDto>(company);

            return Ok(companyDto);
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

            var updatedCompanyDto = _mapper.Map<CompanyDto>(updatedCompany);
            return Ok(updatedCompanyDto);
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

