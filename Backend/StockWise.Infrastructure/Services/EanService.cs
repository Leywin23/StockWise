using Microsoft.EntityFrameworkCore;
using StockWise.Application.Abstractions;
using StockWise.Application.Interfaces;
using StockWise.Infrastructure.Persistence;
using StockWise.Models;
using System.Net.Http;
using System.Text.Json;

namespace StockWise.Infrastructure.Services
{
    public class EanService : IEanService
    {
        private readonly StockWiseDb _context;
        private readonly IHttpClientFactory _httpClient;
        public EanService(StockWiseDb context, IHttpClientFactory httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }
        public async Task<Category> EnsureCategoryHierarchyAsync(string fullCategoryPath)
        {
            var categoryNames = fullCategoryPath.Split(">").Select(s => s.Trim()).ToList();
            Category? parent = null;

            foreach (var name in categoryNames)
            {
                int? parentId = parent?.CategoryId;

                var existing = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name == name && c.ParentId == parentId);

                if (existing == null)
                {
                    var newCategory = new Category
                    {
                        Name = name,
                        Parent = parent
                    };

                    _context.Categories.Add(newCategory);
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

        public async Task<ServiceResult<Product>> GetAndCreateProductByEanAsync(string ean, CancellationToken ct = default)
        {
            var client = _httpClient.CreateClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var url = $"https://api.upcitemdb.com/prod/trial/lookup?upc={ean}";

            try
            {
                var response = await client.GetAsync(url, ct);

                if (!response.IsSuccessStatusCode)
                {
                    return ServiceResult<Product>.BadRequest("API resolution error");
                }

                var content = await response.Content.ReadAsStringAsync(ct);

                using var json = JsonDocument.Parse(content);

                if (!json.RootElement.TryGetProperty("items", out var items) ||
                    items.ValueKind != JsonValueKind.Array ||
                    items.GetArrayLength() == 0)
                {
                    return ServiceResult<Product>.NotFound($"Product with EAN {ean} not found in external API");
                }

                var item = items[0];

                var categoryName = item.GetProperty("category").GetString();
                var category = await EnsureCategoryHierarchyAsync(categoryName);

                decimal lowest = 0m, highest = 0m;

                if (item.TryGetProperty("lowest_recorded_price", out var lowEl) &&
                    lowEl.ValueKind == JsonValueKind.Number)
                {
                    lowest = lowEl.GetDecimal();
                }

                if (item.TryGetProperty("highest_recorded_price", out var highEl) &&
                    highEl.ValueKind == JsonValueKind.Number)
                {
                    highest = highEl.GetDecimal();
                }

                var product = new Product
                {
                    ProductName = item.GetProperty("title").GetString(),
                    Category = category,
                    Description = item.GetProperty("description").GetString(),
                    Image = item.TryGetProperty("images", out var images) &&
                            images.ValueKind == JsonValueKind.Array &&
                            images.GetArrayLength() > 0
                                ? images[0].GetString()
                                : null,
                    EAN = ean,
                    ShoppingPrice = Money.Of(lowest, "EUR"),
                    SellingPrice = Money.Of(highest, "EUR")
                };

                await _context.Products.AddAsync(product, ct);
                await _context.SaveChangesAsync(ct);

                return ServiceResult<Product>.Ok(product);
            }
            catch (Exception ex)
            {
                return ServiceResult<Product>.ServerError($"Server error: {ex.Message}");
            }
        }

    }
}
