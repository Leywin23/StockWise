using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockWise.Models;
using System.Text.Json;

namespace StockWise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EanController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClient;

        public EanController(IHttpClientFactory httpClient)
        {
            _httpClient = httpClient;
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

                var result = new Product
                {
                    ProductName = item.GetProperty("title").GetString(),
                    Category = item.GetProperty("category").GetString(),
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
    }
}
