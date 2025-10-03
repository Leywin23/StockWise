using StockWise.Dtos.CompanyProductDtos;
using StockWise.Models;

namespace StockWise.Dtos.OrderDtos
{
    public class OrderListDto
    {
        public int Id { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public CompanyMiniDto Seller { get; set; }
        public CompanyMiniDto Buyer { get; set; }
        public string UserNameWhoMadeOrder { get; set; }
        public List<ProductWithQuantityDto> ProductsWithQuantity { get; set; }
        public Money TotalPrice { get; set; }
    }
    public class CompanyMiniDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public long NIP { get; set; }
    }
    public class CompanyProductMiniDto
    {
        public int Id { get; set; }
        public string CompanyProductName { get; set; }
        public string EAN { get; set; }
        public Money Price {  get; set; }
    }
}
