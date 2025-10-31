using System.ComponentModel.DataAnnotations;

namespace StockWise.Application.Contracts.CompanyDtos
{
    public class CreateCompanyDto
    {
        public string Name { get; set; }
        [RegularExpression(@"^\d{10}$", ErrorMessage = "NIP must contain exactly 10 digits.")]
        public string NIP { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
