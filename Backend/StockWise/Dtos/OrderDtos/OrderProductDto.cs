namespace StockWise.Dtos.OrderDtos
{
    public class OrderProductDto
    {
        public int CompanyProductId { get; set; }
        public string ProductName { get; set; }
        public string EAN { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
