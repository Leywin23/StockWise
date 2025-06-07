using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Models;
using StockWise.Dtos;
using static System.Net.Mime.MediaTypeNames;
using StockWise.Migrations;

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

            var product = await _context.products
                .Include(p=>p.Category)
                .ThenInclude(c=>c.Parent)
                .FirstOrDefaultAsync(p=>p.ProductId == id);

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
                    current = _context.categories.FirstOrDefault(c => c.CategoryId == current.ParentId);
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
        public async Task<IActionResult> GetProducts()
        {
            List<Product> products = await _context.products.Include(p=>p.Category).ThenInclude(c=>c.Parent).ToListAsync();
            
            var result = new List<object>();

            foreach (var product in products)
            {
                var categoryString = GetCategoryFullPath(product.Category);
                result.Add(new
                {
                    Product = product,
                    CategoryString = categoryString,
                });
            }
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

            var category = await _context.categories.FirstOrDefaultAsync(c=>c.Name == productDto.Category);

            if (category == null) {
                category = new Models.Category { Name = productDto.Category };
                _context.categories.Add(category);
                await _context.SaveChangesAsync();
            }

            var product = new Product
            {
                ProductName = productDto.ProductName,
                EAN = productDto.EAN,
                Image = productDto.Image,
                Description = productDto.Description,
                ShoppingPrice = productDto.ShoppingPrice,
                SellingPrice = productDto.SellingPrice,
                CategoryId = category.CategoryId
            };

            await _context.products.AddAsync(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProductById), new { id = product.ProductId }, product);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (!ModelState.IsValid) {
                return BadRequest();
            }
            var productToDelete = await _context.products.FindAsync(id);
            if (productToDelete == null) {
                return NotFound($"Couldn't find a product with given id: {id}");
            }
            _context.products.Remove(productToDelete);
            await _context.SaveChangesAsync();
            return Ok(productToDelete);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProduct(Product product) {
            var productToUpdate = await _context.products.FirstOrDefaultAsync(x=>x.EAN == product.EAN);
            if (productToUpdate == null) {
                return NotFound($"Couldn't find a product");
            }
            productToUpdate.ProductName = product.ProductName;
            productToUpdate.Category = product.Category;
            productToUpdate.Description = product.Description;
            productToUpdate.ShoppingPrice = product.ShoppingPrice;
            productToUpdate.SellingPrice = product.SellingPrice;
            productToUpdate.Image = product.Image;
            await _context.SaveChangesAsync();
            return Ok(productToUpdate);
        }
    }
}
