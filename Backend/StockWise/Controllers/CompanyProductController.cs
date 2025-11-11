using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StockWise.Application.Contracts.CompanyProductDtos;
using StockWise.Application.Interfaces;
using StockWise.Extensions;
using StockWise.Infrastructure.Persistence;
using StockWise.Infrastructure.Services;
using StockWise.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
        private readonly ICurrentUserService _curr;

        public CompanyProductController(
            StockWiseDb context,
            ICompanyProductService companyProductService,
            MoneyConverter moneyConverter,
            IMapper mapper,
            ICurrentUserService curr)
        {
            _context = context;
            _companyProductService = companyProductService;
            _moneyConverter = moneyConverter;
            _mapper = mapper;
            _curr = curr;
        }

        [HttpGet]
        [Authorize(Roles = "Manager,Worker")]
        public async Task<IActionResult> GetCompanyProducts([FromQuery] CompanyProductQueryParams q, CancellationToken ct = default)
        {
            var user = await _curr.EnsureAsync(ct);
            if (user == null) return Unauthorized(ApiError.From(new Exception("User not found."), StatusCodes.Status401Unauthorized, HttpContext));
            if (user.Company == null) return BadRequest("User is not assigned to any company.");

            var products = await _companyProductService.GetCompanyProductsAsync(user, q);

            return Ok(products);
        }

        [HttpGet("{productId:int}")]
        [Authorize(Roles = "Manager,Worker")]
        public async Task<IActionResult> GetCompanyProductById([FromRoute] int productId)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized(ApiError.From(new Exception("User not found."), StatusCodes.Status401Unauthorized, HttpContext));

            var result = await _companyProductService.GetCompanyProductAsyncById(user, productId);

            return this.ToActionResult(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddCompanyProduct([FromForm] CreateCompanyProductDto dto, CancellationToken ct = default)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
                return Unauthorized(ApiError.From(new Exception("User not found."), StatusCodes.Status401Unauthorized, HttpContext));

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var productDto = await _companyProductService.CreateCompanyProductAsync(dto, user, ct);

            return this.ToActionResult(productDto);
        }

        [HttpDelete("{productId:int}")]
        [Authorize(Roles = "Manager,Worker")]
        public async Task<IActionResult> DeleteCompanyProduct([FromRoute] int productId, CancellationToken ct = default)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized("User not found.");

            var deleted = await _companyProductService.DeleteCompanyProductAsync(user, productId, ct);

            return this.ToActionResult(deleted);
        }

        [HttpPut("{productId:int}")]
        [Authorize(Roles = "Manager,Worker")]
        public async Task<IActionResult> EditCompanyProduct([FromRoute] int productId, [FromForm] UpdateCompanyProductDto productDto, CancellationToken ct = default)
        {
            var user = await GetCurrentUserAsync();

            if (user == null) 
                return Unauthorized(ApiError.From(new Exception("User not found."), StatusCodes.Status404NotFound, HttpContext));

            var result = await _companyProductService.UpdateCompanyProductAsync(productId, user, productDto, ct);

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
            var user = User;
            if(!User.Identity.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("User is not autheticated");
            }
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return null;

            return await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}
