using StockWise.Dtos.AccountDtos;
using StockWise.Dtos.MoneyDtos;
using StockWise.Dtos.OrderDtos;
using StockWise.Models;

namespace StockWise.Dtos.CompanyDtos
{
    public class OrderSummaryDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }      
        public string TotalCurrencyCode { get; set; }
        public string UserNameWhoMadeOrder { get; set; }
        public CompanyMiniDto Counterparty { get; set; }
    }
    public class AdvancedCompanyDto
    {
        public string Name { get; set; }
        public long NIP { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderSummaryDto> OrdersAsBuyer { get; set; } = new();
        public List<OrderSummaryDto> OrdersAsSeller { get; set; } = new();
        public List<CompanyUserDto> Users { get; set; } = new();
    }
}
