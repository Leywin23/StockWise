namespace StockWise.Dtos.OrderDtos
{
    public class CreateOrderDto
    {
        public string SellerName { get; set; }
        public long SellerNIP { get; set; }
        public string Address { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }

        public Dictionary<string, int> ProductsEANWithQuantity { get; set; }
    }
}
