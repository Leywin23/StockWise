namespace StockWise.Dtos.ProductDtos
{
    public class UpdateProductDto
    {
        public string productName { get; set; }
        public string description { get; set; }
        public string ean { get; set; }
        public decimal ShoppingPrice { get; set; } = 0;
        public decimal SellingPrice { get; set; } = 0;
        public string CategoryName { get; set; }
        public string? image { get; set; }
    }
}
