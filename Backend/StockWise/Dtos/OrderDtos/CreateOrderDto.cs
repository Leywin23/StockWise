namespace StockWise.Dtos.OrderDtos
{
    public class CreateOrderDto
    {
        public string SellerName { get; set; }
        public long BuyerNIP { get; set; }
        public long SellerNIP { get; set; }
        public string Address { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string Status { get; set; }

        public List<string> ProductsEAN { get; set; }
        public int Quantity { get; set; } = 0;


    }
}
