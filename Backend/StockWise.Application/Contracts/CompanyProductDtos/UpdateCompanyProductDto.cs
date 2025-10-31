using Microsoft.AspNetCore.Http;
using StockWise.Models;

namespace StockWise.Application.Contracts.CompanyProductDtos
{
    public class UpdateCompanyProductDto
    {
        public string CompanyProductName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public string CategoryName { get; set; }
        public IFormFile? ImageFile { get; set; }
        public int Stock { get; set; }
        public bool IsAvailableForOrder {  get; set; }
    }
}
