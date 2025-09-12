using StockWise.Models;

namespace StockWise.Dtos.CompanyProductDtos
{
    public class UpdateCompanyProductDto
    {
        public string CompanyProductName { get; set; }
        public string Description { get; set; }
        public string Ean { get; set; }
        public decimal Price { get; set; }
        public Currency Currency { get; set; }
        public string CategoryName { get; set; }
        public string? Image { get; set; }
        public int Stock { get; set; }
        public bool IsAvailableForOrder {  get; set; }
    }
}
