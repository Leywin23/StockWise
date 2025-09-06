using StockWise.Models;

namespace StockWise.Dtos.CompanyProductDtos
{
    public class CompanyProductDto
    {
        public string CompanyProductName { get; set; }
        public string EAN { get; set; }
        public string? Image { get; set; }
        public string Description { get; set; }
        public Money ShoppingPrice { get; set; }
        public Money SellingPrice { get; set; }
        public int Stock { get; set; } = 0;
        public bool IsAvailableForOrder { get; set; }
    }
}
