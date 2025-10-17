using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Dtos.CompanyDtos;
using StockWise.Extensions;
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
        private readonly ICurrentUserService _currentUserService;

        public CompanyController(UserManager<AppUser> userManager, ICompanyService companyService, StockWiseDb context, IMapper mapper, ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _companyService = companyService;
            _context = context;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCompanyData()
        {
            var user = await _currentUserService.EnsureAsync();
            if (user == null) return Unauthorized(ApiError.From(new Exception("User not found."), StatusCodes.Status401Unauthorized, HttpContext));

            var companyDto = await _companyService.GetCompanyAsync(user);

            return this.ToActionResult(companyDto);
        }
        [Authorize]
        [HttpGet("me/advanced")]
        public async Task<IActionResult> GetAdvancedCompanyData()
        {
            var user = await _currentUserService.EnsureAsync();
            if (user == null) return Unauthorized(ApiError.From(new Exception("User not found."), StatusCodes.Status401Unauthorized, HttpContext));

            var companyData = await _companyService.GetAdvancedCompanyDataAsync(user);
            return this.ToActionResult(companyData);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllCompaniesData([FromQuery] CompanyQueryParams q)
        {
            var companiesData = await _companyService.GetAllCompanyAsync(q);
            return this.ToActionResult(companiesData);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateCompany(CreateCompanyDto companyDto)
        {
            var newCompanyDto = await _companyService.CreateCompanyAsync(companyDto);
            return this.ToActionResult(newCompanyDto);
        }

        [Authorize(Roles = "Manager")]
        [HttpDelete]
        public async Task<IActionResult> DeleteCompany()
        {
            var user = await _currentUserService.EnsureAsync();
            if (user == null) return Unauthorized(ApiError.From(new Exception("User not found"), StatusCodes.Status401Unauthorized, HttpContext));

            var companyDto = await _companyService.DeleteCompanyAsync(user);
            return this.ToActionResult(companyDto);
        }

        [Authorize(Roles = "Manager")]
        [HttpPut]
        public async Task<IActionResult> EditCompany(UpdateCompanyDto companyDto)
        {
            var user = await _currentUserService.EnsureAsync();
            if (user == null) return Unauthorized(ApiError.From(new Exception("User not found"), StatusCodes.Status401Unauthorized, HttpContext));

            var updatedCompanyDto = await _companyService.UpdateCompanyAsync(companyDto, user);
            
            return this.ToActionResult(updatedCompanyDto);
        }


    }
}

