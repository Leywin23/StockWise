using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Dtos.CompanyProductDtos;
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

        public CompanyProductController(StockWiseDb context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

            var products = await _context.CompanyProducts.Where(cp => cp.Company.Id == company.Id).ToListAsync();

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

            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == user.Company.Id);
            if (company == null)
                return BadRequest("User is not assigned to any company.");

            var product = await _context.CompanyProducts.FirstOrDefaultAsync(cp => cp.Company.Id == company.Id);

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

            var newCompanyProduct = new CompanyProduct
            {
                CompanyProductName = productDto.ProductName,
                EAN = productDto.EAN,
                Image = productDto.Image,
                Description = productDto.Description,
                ShoppingPrice = productDto.ShoppingPrice,
                SellingPrice = productDto.SellingPrice,
                Stock = productDto.Stock,
                Company = company,
                IsAvailableForOrder = productDto.IsAvailableForOrder
    };

            await _context.CompanyProducts.AddAsync(newCompanyProduct);
            await _context.SaveChangesAsync();

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

            var ProductToDelete = companyProducts.FirstOrDefault(cp=>cp.CompanyProductId==ProductId);
            if (ProductToDelete == null) {
                return NotFound("Product not found");
            }
            _context.CompanyProducts.Remove(ProductToDelete);
            await _context.SaveChangesAsync();
            return Ok(ProductToDelete);
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> EditCompanyProduct(int productId, CompanyProduct productDto)
        {
            var userName = User.FindFirst(ClaimTypes.Name).Value;
            if (string.IsNullOrEmpty(userName)) {
                return Unauthorized("Username not found in token.");
            };

            var user = await _userManager.Users
                .Include(u => u.Company)
                .ThenInclude(c => c.CompanyProducts)
                .FirstOrDefaultAsync(u=>u.UserName == userName);

            if (user == null) {
                return Unauthorized("User not found.");
            }

            var company = user.Company;
            if (company == null)
            {
                return BadRequest("User is not assigned to any company.");
            }

            var product = company.CompanyProducts.FirstOrDefault(cp=>cp.CompanyProductId==productId);
            if (product == null) {
                return NotFound($"The product with id {productId} does not belong to this company");
            }

            var duplicate = company.CompanyProducts.Any(cp => cp.CompanyProductId != productId &&
            (cp.EAN == productDto.EAN || cp.CompanyProductName == productDto.CompanyProductName));

            if (duplicate) {
                return BadRequest("Another product with the same EAN or name already exists in your company.");
            }


            product.CompanyProductName = productDto.CompanyProductName;
            product.EAN = productDto.EAN;
            product.Image = productDto.Image;
            product.Description = productDto.Description;
            product.ShoppingPrice = productDto.ShoppingPrice;
            product.SellingPrice = productDto.SellingPrice;
            product.Stock = productDto.Stock;
            product.IsAvailableForOrder = productDto.IsAvailableForOrder;

            await _context.SaveChangesAsync();

            return Ok(product);
        }
    }
}
