using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StockWise.Models
{
    public class Product
    {
        public int ProductId {  get; set; }
        [Required, StringLength(200)]
        public string ProductName { get; set; } = default!;
        [Required, RegularExpression(@"^\d{8}$|^\d{13}$")]
        public string EAN { get; set; } = default!;
        public string? Image { get; set; }
        [Required, StringLength(2000)]
        public string Description { get; set; } = default!;
        [Range(0, Double.MaxValue)]
        public Money ShoppingPrice { get; set; } = default!;
        [Range(0, Double.MaxValue)]
        public Money SellingPrice { get; set; } = default!;
        public int CategoryId {  get; set; }
        [Required]
        [JsonPropertyName("category")]
        public Category Category { get; set; } = default!;
    }
}
