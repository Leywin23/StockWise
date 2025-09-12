using StockWise.Models;

namespace StockWise.Dtos.CompanyProductDtos
{
    public class CreateCompanyProductDto
    {
        public string ProductName { get; set; }
        public string EAN { get; set; }
        public string? Image { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public Currency Currency { get; set; }
        public int Stock { get; set; }
        public bool IsAvailableForOrder { get; set; }
    }
}
