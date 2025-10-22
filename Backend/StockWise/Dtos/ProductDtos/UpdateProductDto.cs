using StockWise.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StockWise.Dtos.ProductDtos
{
    public class UpdateProductDto
    {
        [Required, StringLength(200)]
        public string ProductName { get; set; } = default!;
        [Required, StringLength(2000)]
        public string Description { get; set; } = default!;

        [Range(0, double.MaxValue)]
        public decimal ShoppingPrice { get; set; } = 0;
        [Range(0, double.MaxValue)]
        public decimal SellingPrice { get; set; } = 0;
        [Required]
        public string Currency { get; set; } = default!;
        [Required]
        [JsonPropertyName("category")]
        public string CategoryName { get; set; } = default!;
        public IFormFile? Image { get; set; }
    }
}
