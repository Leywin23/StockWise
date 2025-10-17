using StockWise.Models;

namespace StockWise.Dtos.OrderDtos
{
    public class OrderDto
    {
        public Company Seller { get; set; }
        public Company Buyer { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string UserNameWhoMadeOrder { get; set; }
        public List<OrderProduct> ProductsWithQuantity { get; set; } = new List<OrderProduct>();
        public Money TotalPrice { get; set; }
    }
}
