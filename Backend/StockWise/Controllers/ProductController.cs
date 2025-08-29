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

namespace StockWise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly StockWiseDb _context;
        public ProductController(StockWiseDb context)
        {
            _context = context;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .ThenInclude(c => c.Parent)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            var categoryFullPath = GetCategoryFullPath(product.Category);

            var result = new
            {
                Product = product,
                CategoryPath = categoryFullPath,
            };

            return Ok(result);
        }

        private string GetCategoryFullPath(Models.Category category)
        {
            var names = new List<string>();
            var current = category;

            while (current != null)
            {
                names.Add(current.Name);
                if (current.Parent == null && current.ParentId != null)
                {
                    current = _context.Categories.FirstOrDefault(c => c.CategoryId == current.ParentId);
                }
                else
                {
                    current = current.Parent;
                }
            }
            names.Reverse();
            return string.Join(" > ", names);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products.Include(p => p.Category).ThenInclude(c => c.Parent).ToListAsync();

            var result = products.Select(p => new ProductDto
            {

                id = p.ProductId,
                ProductName = p.ProductName,
                Ean = p.EAN,
                Description = p.Description,
                SellingPrice = p.SellingPrice.amount,
                ShoppingPrice = p.ShoppingPrice.amount,
                Currency = p.SellingPrice.currency,
                CategoryString = GetCategoryFullPath(p.Category),
            });

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] CreateProductDto productDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            if (productDto == null) {
                return BadRequest("Product data is required.");
            }

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == productDto.Category);

            if (category == null) {
                category = new Models.Category { Name = productDto.Category };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
            }

            var product = productDto.ToProductFromCreate(category);

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProductById), new { id = product.ProductId }, product);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (!ModelState.IsValid) {
                return BadRequest();
            }
            var productToDelete = await _context.Products.FindAsync(id);
            if (productToDelete == null) {
                return NotFound($"Couldn't find a product with given id: {id}");
            }
            _context.Products.Remove(productToDelete);
            await _context.SaveChangesAsync();
            return Ok(productToDelete);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductDto productDto)
        {
            var productToUpdate = await _context.Products.FirstOrDefaultAsync(x => x.EAN == productDto.ean);
            if (productToUpdate == null)
                return NotFound("Couldn't find a product");

            var category = await _context.Categories.FirstOrDefaultAsync(x => x.Name == productDto.CategoryName);
            if (category == null)
            {
                return NotFound("Coundn't find a category with this name");
            }
            var shoppingPrice = Money.Of(productDto.ShoppingPrice, productDto.Currency.CurrencyCode);
            var sellingPrice = Money.Of(productDto.SellingPrice, productDto.Currency.CurrencyCode);

            productToUpdate.ProductName = productDto.productName;
            productToUpdate.CategoryId = category.CategoryId;
            productToUpdate.Description = productDto.description;
            productToUpdate.ShoppingPrice = shoppingPrice;
            productToUpdate.SellingPrice = sellingPrice;
            productToUpdate.Image = productDto.image;

            _context.Products.Update(productToUpdate);
            await _context.SaveChangesAsync();
            return Ok(productToUpdate);
        }
    }
}
