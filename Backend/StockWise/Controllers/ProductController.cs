using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Models;
using static System.Net.Mime.MediaTypeNames;
using StockWise.Migrations;
using StockWise.Mappers;
using Microsoft.AspNetCore.Authorization;
using StockWise.Dtos.ProductDtos;
using AutoMapper;
using StockWise.Interfaces;

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
        public async Task<IActionResult> AddProduct([FromBody] CreateProductDto productDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var product = await _productService.AddProduct(productDto);

            return Ok(product);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (!ModelState.IsValid) {
                return BadRequest();
            }
            var productToDelete = await _productService.DeleteProduct(id);
            return Ok(productToDelete);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductDto productDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var productToUpdate = await _productService.UpdateProduct(productDto);
            return Ok(productToUpdate);
        }
    }
}
