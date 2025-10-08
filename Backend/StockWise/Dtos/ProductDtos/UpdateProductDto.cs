using StockWise.Models;

namespace StockWise.Dtos.ProductDtos
{
    public class UpdateProductDto
    {
        public string ProductName { get; set; }
        public string Description { get; set; }
        public string EAN { get; set; }
        public decimal ShoppingPrice { get; set; } = 0;
        public decimal SellingPrice { get; set; } = 0;
        public Currency Currency { get; set; }
        public string CategoryName { get; set; }
        public string? image { get; set; }
    }
}
