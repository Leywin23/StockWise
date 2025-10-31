using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using StockWise.Application.Interfaces;
using StockWise.Infrastructure.Persistence;
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
        private readonly IEanService _eanService;

        public EanController(IHttpClientFactory httpClient, StockWiseDb context, IEanService eanService)
        {
            _httpClient = httpClient;
            _context = context; 
            _eanService = eanService;
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
                var category = await _eanService.EnsureCategoryHierarchyAsync(categoryName);

                decimal lowest = 0m, highest = 0m;
                if(item.TryGetProperty("lowest_recorded_price", out var lowEl) && lowEl.ValueKind == JsonValueKind.Number) 
                    lowest = lowEl.GetDecimal();

                if(item.TryGetProperty("highest_recorded_price", out var highEl) && highEl.ValueKind == JsonValueKind.Number)
                    highest = highEl.GetDecimal();

                var result = new Product
                {
                    ProductName = item.GetProperty("title").GetString(),
                    Category = category,
                    Description = item.GetProperty("description").GetString(),
                    Image = item.TryGetProperty("images", out var images) && images.GetArrayLength() > 0 ? images[0].GetString() : null,
                    EAN = ean,
                    ShoppingPrice = Money.Of(lowest, "EUR"),
                    SellingPrice = Money.Of(highest,"EUR")
                };

                await _context.Products.AddAsync(result);
                await _context.SaveChangesAsync();
                return Ok(result);
            }
            catch (Exception ex) {
                return StatusCode(500, $"Server Error: {ex.Message}");
            }

        }

    }
}
