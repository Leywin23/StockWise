using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Dtos.CompanyProductDtos;
using StockWise.Dtos.ProductDtos;
using StockWise.Helpers;
using StockWise.Interfaces;
using StockWise.Mappers;
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
        [Authorize(Roles = "Manager,Worker")]
        public async Task<IActionResult> GetCompanyProducts([FromQuery] CompanyProductQueryParams q)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized(ApiError.From(new Exception("User not found."), StatusCodes.Status401Unauthorized, HttpContext));
            if (user.Company == null) return BadRequest("User is not assigned to any company.");

            var products = await _companyProductService.GetCompanyProductsAsync(user, q);
            var productsDto = _mapper.Map<IEnumerable<CompanyProductDto>>(products);
            return Ok(productsDto);
        }

        [HttpGet("{productId:int}")]
        [Authorize(Roles = "Manager,Worker")]
        public async Task<IActionResult> GetCompanyProductById([FromRoute] int productId)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized(ApiError.From(new Exception("User not found."), StatusCodes.Status401Unauthorized, HttpContext));

            var result = await _companyProductService.GetCompanyProductAsyncById(user, productId);
            if (result.IsSuccess)
            {
                var dto = _mapper.Map<CompanyProductDto>(result);
                return Ok(dto);
            }
            return this.ToActionResult(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddCompanyProduct([FromBody] CreateCompanyProductDto dto)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
                return Unauthorized(ApiError.From(new Exception("User not found."), StatusCodes.Status401Unauthorized, HttpContext));

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var productDto = await _companyProductService.CreateCompanyProductAsync(dto, user);

            return this.ToActionResult(productDto);
        }

        [HttpDelete("{productId:int}")]
        [Authorize(Roles = "Manager,Worker")]
        public async Task<IActionResult> DeleteCompanyProduct([FromRoute] int productId)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized("User not found.");

            var deleted = await _companyProductService.DeleteCompanyProductAsync(user, productId);
            if (deleted == null) return NotFound("Product not found.");
            var deletedProductDto = _mapper.Map<CompanyProductDto>(deleted);
            return Ok(deletedProductDto);
        }

        [HttpPut("{productId:int}")]
        [Authorize(Roles = "Manager,Worker")]
        public async Task<IActionResult> EditCompanyProduct([FromRoute] int productId, [FromBody] UpdateCompanyProductDto productDto)
        {
            var user = await GetCurrentUserAsync();

            if (user == null) 
                return Unauthorized(ApiError.From(new Exception("User not found."), StatusCodes.Status404NotFound, HttpContext));

            var result = await _companyProductService.UpdateCompanyProductAsync(productId, user, productDto);

            return this.ToActionResult(result);
        }

        [HttpGet("{productId:int}/convert")]
        [Authorize(Roles = "Manager,Worker")]
        public async Task<IActionResult> ConvertToAnotherCurrency([FromRoute] int productId, [FromQuery] string toCode)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized("User not found.");



            if (string.IsNullOrWhiteSpace(toCode)) return BadRequest("Target currency code is required.");

            var result = await _companyProductService.GetCompanyProductAsyncById(user, productId);
            if(!result.IsSuccess)
                return this.ToActionResult(result);

            var convertedPrice = await _moneyConverter.ConvertAsync(result.Value.Price, toCode);
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
