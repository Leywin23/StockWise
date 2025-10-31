using StockWise.Models;

namespace StockWise.Application.Contracts.OrderDtos
{
    public class OrderDto
    {
        public CompanyMiniDto Seller { get; set; }
        public CompanyMiniDto Buyer { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string UserNameWhoMadeOrder { get; set; }
        public List<OrderProductDto> Products { get; set; } = new();
        public Money TotalPrice { get; set; }
    }
}
