namespace StockWise.Dtos.ProductDtos
{
    public class CreateProductDto
    {
        public string ProductName { get; set; }
        public string EAN { get; set; }
        public string? Image { get; set; }
        public string Description { get; set; }
        public decimal ShoppingPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public string Category { get; set; }
        public int Stock { get; set; }
    }
}
