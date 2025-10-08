using StockWise.Models;

namespace StockWise.Dtos.ProductDtos
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string Ean { get; set; }
        public string Description { get; set; }
        public int Stock { get; set; }
        public decimal ShoppingPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public Currency Currency { get; set; }
        public string CategoryString { get; set; }
    }
}
