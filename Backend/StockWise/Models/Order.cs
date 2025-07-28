namespace StockWise.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int SellerId {  get; set; }
        public Company Seller { get; set; }

        public int BuyerId {  get; set; }
        public Company Buyer { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Product> Products { get; set; }
    }
}
