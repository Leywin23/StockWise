using System.ComponentModel.DataAnnotations;

namespace StockWise.Models
{
    public class CompanyProduct
    {
        public int CompanyProductId { get; set; }

        [Required]
        [StringLength(200)]
        public string CompanyProductName { get; set; } = default!;

        [Required]
        [RegularExpression(@"^\d{8}$|^\d{13}$", ErrorMessage = "EAN must contain exactly 8 or 13 digits.")]
        public string EAN { get; set; } = default!;

        public string? Image { get; set; }

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = default!;

        [Required]
        public Money Price { get; set; } = default!;

        public int Stock { get; set; } = 0;

        public int CompanyId { get; set; }
        public Company Company { get; set; } = default!;

        public bool IsAvailableForOrder { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; } = default!;
        public bool IsDeleted { get; set; } = false;

        public ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
    }
}
