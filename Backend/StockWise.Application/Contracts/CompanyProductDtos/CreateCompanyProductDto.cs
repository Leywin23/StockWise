using Microsoft.AspNetCore.Http;
using StockWise.Models;
using System.ComponentModel.DataAnnotations;

namespace StockWise.Application.Contracts.CompanyProductDtos
{
    public class CreateCompanyProductDto
    {
        [Required]
        public string CompanyProductName { get; set; } = default!;

        [Required]
        [RegularExpression(@"^\d{8}$|^\d{13}$", ErrorMessage = "EAN must contain exactly 8 or 13 digits.")]
        public string EAN { get; set; } = default!;

        public IFormFile? ImageFile { get; set; }

        [Required]
        public string Description { get; set; } = default!;

        [Required]
        public string Category { get; set; } = default!;

        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal Price { get; set; }

        [Required]
        public string Currency { get; set; } = default!;

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        public bool IsAvailableForOrder { get; set; }
    }
}
