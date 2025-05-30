namespace StockWise.Dtos
{
    public class CreateProductDto
    {
        public string ProductName { get; set; }
        public string EAN { get; set; }
        public string? Image { get; set; }
        public string Description { get; set; }
        public double ShoppingPrice { get; set; }
        public double SellingPrice { get; set; }
        public string Category { get; set; }
    }
}
