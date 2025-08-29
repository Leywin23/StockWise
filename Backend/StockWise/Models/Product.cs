namespace StockWise.Models
{
    public class Product
    {
        public int ProductId {  get; set; }
        public string ProductName { get; set; }
        public string EAN {  get; set; }
        public string? Image { get; set; }
        public string Description {  get; set; }
        public Money ShoppingPrice { get; set; }
        public Money SellingPrice { get; set; }
        public int CategoryId {  get; set; }
        public Category Category { get; set; }
    }
}
