using StockWise.Models;
using System.ComponentModel.DataAnnotations;

namespace StockWise.Dtos.CompanyProductDtos
{
    public class CreateCompanyProductDto
    {
        [Required]
        public string CompanyProductName { get; set; } = default!;

        [Required]
        public string EAN { get; set; } = default!;

        public string? Image { get; set; }

        [Required]
        public string Description { get; set; } = default!;

        [Required]
        public string Category { get; set; } = default!;

        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal Price { get; set; }

        [Required]
        public Currency Currency { get; set; } = default!; // zakładam: public class Currency { public string Code { get; set; } }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        public bool IsAvailableForOrder { get; set; }
    }
}
