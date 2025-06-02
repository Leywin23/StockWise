namespace StockWise.Models
{
    public class Product
    {
        public int ProductId {  get; set; }
        public string ProductName { get; set; }
        public string EAN {  get; set; }
        public string? Image { get; set; }
        public string Description {  get; set; }
        public decimal ShoppingPrice { get; set; } = 0;
        public decimal SellingPrice { get; set; } = 0;
        public string Category { get; set; }
    }
}
