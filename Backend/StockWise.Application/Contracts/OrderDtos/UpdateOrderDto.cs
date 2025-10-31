namespace StockWise.Application.Contracts.OrderDtos
{
    public class UpdateOrderDto
    {
        public string Currency { get; set; }
        public Dictionary<string, int> ProductsEANWithQuantity { get; set; }
    }
}
