using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Models;
using System.Text.Json;

namespace StockWise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EanController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly StockWiseDb _context;

        public EanController(IHttpClientFactory httpClient, StockWiseDb context)
        {
            _httpClient = httpClient;
            _context = context; 
        }

        [HttpGet("{ean}")]
        public async Task<IActionResult> GetProductByEan(string ean) {
            var client = _httpClient.CreateClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var url = $"https://api.upcitemdb.com/prod/trial/lookup?upc={ean}";

            try
            {
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode) return StatusCode((int)response.StatusCode, "API resolution error");

                var content = await response.Content.ReadAsStringAsync(); 
                var json = JsonDocument.Parse(content);
                var item = json.RootElement.GetProperty("items")[0];
                var categoryName = item.GetProperty("category").GetString();
                var category = await EnsureCategoryHierarchyAsync(categoryName);


                var result = new Product
                {
                    ProductName = item.GetProperty("title").GetString(),
                    Category = category,
                    Description = item.GetProperty("description").GetString(),
                    Image = item.TryGetProperty("images", out var images) && images.GetArrayLength() > 0 ? images[0].GetString() : null,
                    EAN = ean
                };
                return Ok(result);
            }
            catch (Exception ex) {
                return StatusCode(500, $"Server Error: {ex.Message}");
            }

        }


        private async Task<Category> EnsureCategoryHierarchyAsync(string fullCategoryPath)
        {
            var categoryNames = fullCategoryPath.Split(">").Select(s => s.Trim()).ToList();
            Category? parent = null;

            foreach (var name in categoryNames)
            {
                int? parentId = parent?.CategoryId;

                var existing = await _context.categories
                    .FirstOrDefaultAsync(c => c.Name == name && c.ParentId == parentId);

                if (existing == null)
                {
                    var newCategory = new Category
                    {
                        Name = name,
                        Parent = parent
                    };

                    _context.categories.Add(newCategory);
                    await _context.SaveChangesAsync();
                    parent = newCategory;
                }
                else
                {
                    parent = existing;
                }
            }

            return parent!;
        }

    }
}
