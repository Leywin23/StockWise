namespace StockWise.Application.Contracts.OrderDtos
{
    public class ProductWithQuantityDto
    {
        public CompanyProductMiniDto Product { get; set; }
        public int Quantity { get; set; }

    }
}
