namespace StockWise.Models
{
    public enum OrderStatus
    {
        Pending = 10,
        Accepted = 20,
        Rejected = 30,
        Canceled = 40,
        Completed = 50
    };

    public class Order
    {
        public int Id { get; set; }
        public int SellerId {  get; set; }
        public Company Seller { get; set; }

        public int BuyerId {  get; set; }
        public Company Buyer { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string UserNameWhoMadeOrder { get; set; }
        public List<OrderProduct> ProductsWithQuantity { get; set; } = new List<OrderProduct>();
        public Money TotalPrice { get; set; }
    }
}
