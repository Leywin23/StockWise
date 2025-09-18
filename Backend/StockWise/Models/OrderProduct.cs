namespace StockWise.Models
{
    public class OrderProduct
    {
        public int OrderId { get; set; }
        public Order OrderTable { get; set; }

        public int CompanyProductId { get; set; }
        public CompanyProduct CompanyProduct { get; set; }

        public int Quantity { get; set; }
    }
}
