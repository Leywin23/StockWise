using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Dtos.CompanyProductDtos;
using StockWise.Dtos.ProductDtos;
using StockWise.Interfaces;
using StockWise.Models;
using System.Security.Claims;

namespace StockWise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyProductController : ControllerBase
    {
        private readonly StockWiseDb _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ICompanyProductService _companyProductService;

        public CompanyProductController(StockWiseDb context, UserManager<AppUser> userManager, ICompanyProductService companyProductService)
        {
            _context = context;
            _userManager = userManager;
            _companyProductService = companyProductService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetCompanyProducts() {


            var userName = User.FindFirst(ClaimTypes.Name).Value;
            if (string.IsNullOrEmpty(userName)) {
                return Unauthorized("Username not found in token");
            }
            var user = await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.UserName == userName);

            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == user.Company.Id);
            if (company == null)
                return BadRequest("User is not assigned to any company.");

            var products = await _companyProductService.GetCompanyProductsAsync(user);

            return Ok(products);
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCompanyProductById(int productId)
        {

            var userName = User.FindFirst(ClaimTypes.Name).Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("Username not found in token");
            }
            var user = await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.UserName == userName);

            var product = await _companyProductService.GetCompanyProductAsync(user, productId);

            return Ok(product);

        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddCompanyProduct(CreateCompanyProductDto productDto)
        {
            var userName = User.FindFirst(ClaimTypes.Name).Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("Username not found in token");
            }

            var user = await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.UserName == userName);

            if (user == null)
                return Unauthorized("User not found.");

            var company = user.Company;

            if (company == null)
                return BadRequest("User is not assigned to any company.");

            var productExists = await _context.CompanyProducts
                .AnyAsync(cp => cp.Company.Id == company.Id &&
                                (cp.EAN == productDto.EAN || cp.CompanyProductName == productDto.ProductName));

            if (productExists)
            {
                return BadRequest("Product is already in company stock.");
            }

            var newCompanyProduct = await _companyProductService.CreateCompanyProductAsync(company, productDto);

            return Ok(newCompanyProduct);
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteCompanyProduct(int ProductId)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("Username not found in token.");
            }
            var user = await _userManager.Users.Include(u=>u.Company).ThenInclude(c => c.CompanyProducts).FirstOrDefaultAsync(u=>u.UserName==userName);
            if (user == null) {
                return Unauthorized("User not found.");
            }
            var company = user.Company;
            if (company == null) {
                return BadRequest("User is not assigned to any company.");
            }

            var companyProducts = company.CompanyProducts.ToList();

            var ProductToDelete = await _companyProductService.DeleteCompanyProductAsync(user, ProductId);
            return Ok(ProductToDelete);
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> EditCompanyProduct(int productId, UpdateCompanyProductDto productDto)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
                return Unauthorized("Username not found in token.");

            var user = await _userManager.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.UserName == userName);

            if (user == null)
                return Unauthorized("User not found.");

            try
            {
                var updatedProduct = await _companyProductService.UpdateCompanyProductAsync(productId, user, productDto);
                if (updatedProduct == null)
                    return NotFound("Product not found or user not assigned to any company.");

                return Ok(updatedProduct);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}
