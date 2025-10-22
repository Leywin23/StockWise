using StockWise.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StockWise.Dtos.ProductDtos
{
    public class CreateProductDto
    {
        [Required, StringLength(200)]
        public string ProductName { get; set; } = default!;
        [Required, RegularExpression(@"^\d{8}$|^\d{13}$")]
        public string EAN { get; set; } = default!;
        public IFormFile? Image { get; set; }
        [Required, StringLength(2000)]
        public string Description { get; set; } = default!;
        [Range(0, double.MaxValue)]
        public decimal ShoppingPrice { get; set; }
        [Range(0, double.MaxValue)]
        public decimal SellingPrice { get; set; }
        [Required]
        public string Currency { get; set; } = default!;

        [Required]
        [JsonPropertyName("category")]
        public string Category { get; set; } = default!;
    }
}
