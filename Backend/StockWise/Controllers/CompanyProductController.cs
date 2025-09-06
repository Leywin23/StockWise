using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Dtos.CompanyProductDtos;
using StockWise.Dtos.ProductDtos;
using StockWise.Interfaces;
using StockWise.Models;
using StockWise.Services;

namespace StockWise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanyProductController : ControllerBase
    {
        private readonly StockWiseDb _context;
        private readonly ICompanyProductService _companyProductService;
        private readonly MoneyConverter _moneyConverter;
        private readonly IMapper _mapper;

        public CompanyProductController(
            StockWiseDb context,
            ICompanyProductService companyProductService,
            MoneyConverter moneyConverter,
            IMapper mapper)
        {
            _context = context;
            _companyProductService = companyProductService;
            _moneyConverter = moneyConverter;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanyProducts()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized("User not found.");
            if (user.Company == null) return BadRequest("User is not assigned to any company.");

            var products = await _companyProductService.GetCompanyProductsAsync(user);
            var productsDto = _mapper.Map<IEnumerable<CompanyProductDto>>(products);
            return Ok(productsDto);
        }

        [HttpGet("{productId:int}")]
        public async Task<IActionResult> GetCompanyProductById([FromRoute] int productId)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized("User not found.");

            var product = await _companyProductService.GetCompanyProductAsync(user, productId);
            if (product == null) return NotFound("Product not found.");
            var productDto = _mapper.Map<CompanyProductDto>(product);
            return Ok(productDto);
        }

        [HttpPost]
        public async Task<IActionResult> AddCompanyProduct([FromBody] CreateCompanyProductDto productDto)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized("User not found.");
            var company = user.Company;
            if (company == null) return BadRequest("User is not assigned to any company.");

            var productExists = await _context.CompanyProducts
                .AnyAsync(cp => cp.Company.Id == company.Id &&
                                (cp.EAN == productDto.EAN ||
                                 cp.CompanyProductName == productDto.ProductName));
            if (productExists) return BadRequest("Product is already in company stock.");

            var newCompanyProduct = await _companyProductService.CreateCompanyProductAsync(company, productDto);
            var newCompanyProductDto = _mapper.Map<CompanyProductDto>(newCompanyProduct);
            return Ok(newCompanyProductDto);
        }

        [HttpDelete("{productId:int}")]
        public async Task<IActionResult> DeleteCompanyProduct([FromRoute] int productId)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized("User not found.");

            var deleted = await _companyProductService.DeleteCompanyProductAsync(user, productId);
            if (deleted == null) return NotFound("Product not found.");
            var deletedProductDto = _mapper.Map<CompanyProductDto>(deleted);
            return Ok(deleted);
        }

        [HttpPut("{productId:int}")]
        public async Task<IActionResult> EditCompanyProduct([FromRoute] int productId, [FromBody] UpdateCompanyProductDto productDto)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized("User not found.");

            try
            {
                var updated = await _companyProductService.UpdateCompanyProductAsync(productId, user, productDto);
                if (updated == null) return NotFound("Product not found or user not assigned to any company.");
                var updatedDto = _mapper.Map<CompanyProductDto>(updated);
                return Ok(updatedDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{productId:int}/convert")]
        public async Task<IActionResult> ConvertToAnotherCurrency([FromRoute] int productId, [FromQuery] string toCode)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized("User not found.");

            var product = await _companyProductService.GetCompanyProductAsync(user, productId);
            if (product == null) return NotFound("Product not found.");

            if (string.IsNullOrWhiteSpace(toCode)) return BadRequest("Target currency code is required.");

            var convertedPrice = await _moneyConverter.ConvertAsync(product.SellingPrice, toCode);
            return Ok(convertedPrice);
        }

        private async Task<AppUser?> GetCurrentUserAsync()
        {
            var userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName)) return null;

            return await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.UserName == userName);
        }
    }
}
