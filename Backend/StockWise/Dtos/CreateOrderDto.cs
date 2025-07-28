namespace StockWise.Dtos
{
    public class CreateOrderDto
    {
        public string SellerName {  get; set; }
        public string BuyerNIP {  get; set; }
        public string SellerNIP { get; set; }
        public string Address { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string Status {  get; set; }

        public List<String> ProductsEAN {  get; set; }
        public int Quantity { get; set; } = 0;
    }
}
