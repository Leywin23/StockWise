using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockWise.Models;
using static System.Net.Mime.MediaTypeNames;
using StockWise.Migrations;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using StockWise.Application.Contracts.ProductDtos;
using StockWise.Infrastructure.Persistence;
using StockWise.Application.Interfaces;

namespace StockWise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly StockWiseDb _context;
        private readonly IMapper _mapper;
        private readonly IProductService _productService;
        public ProductController(StockWiseDb context, IMapper mapper, IProductService productService)
        {
            _context = context;
            _mapper = mapper;
            _productService = productService;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await _productService.GetProductById(id);

            return Ok(result);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetProducts()
        {
            var result = await _productService.GetProducts();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromForm] CreateProductDto productDto, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var product = await _productService.AddProduct(productDto, ct);

            return Ok(product);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct([FromRoute] int id, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) {
                return BadRequest();
            }
            var productToDelete = await _productService.DeleteProduct(id, ct);
            return Ok(productToDelete);
        }

        [HttpPut("{productId:int}")]
        public async Task<IActionResult> UpdateProduct([FromRoute] int productId, [FromForm] UpdateProductDto productDto, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var productToUpdate = await _productService.UpdateProduct(productId, productDto, ct);
            return Ok(productToUpdate);
        }
    }
}
