using System.ComponentModel.DataAnnotations;

namespace StockWise.Models
{
    public class Company
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = default!;

        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "NIP must contain exactly 10 digits.")]
        public string NIP { get; set; } = default!;

        [Required]
        [StringLength(300)]
        public string Address { get; set; } = default!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        [Phone]
        public string Phone { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool Verified { get; set; } = false;

        public ICollection<Order> OrdersAsBuyer { get; set; } = new List<Order>();
        public ICollection<Order> OrdersAsSeller { get; set; } = new List<Order>();
        public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
        public ICollection<CompanyProduct> CompanyProducts { get; set; } = new List<CompanyProduct>();
    }
}
