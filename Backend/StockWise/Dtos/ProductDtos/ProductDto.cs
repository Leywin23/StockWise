using StockWise.Models;
using System.ComponentModel.DataAnnotations;

namespace StockWise.Dtos.ProductDtos
{
    public class ProductDto
    {
        [Required, StringLength(200)]
        public string ProductName { get; set; } = default!;
        [Required, RegularExpression(@"^\d{8}$|^\d{13}$")]
        public string Ean { get; set; } = default!;
        [Required, StringLength(2000)]
        public string Description { get; set; }
        [Range(0, double.MaxValue)]
        public decimal ShoppingPrice { get; set; }
        [Range(0, double.MaxValue)]
        public decimal SellingPrice { get; set; }
        [Required]
        public Currency Currency { get; set; } = default!;

        [Required, StringLength(200)]
        public string CategoryString { get; set; } = default!;
    }
}
