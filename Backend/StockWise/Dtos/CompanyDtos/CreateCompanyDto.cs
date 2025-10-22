using System.ComponentModel.DataAnnotations;

namespace StockWise.Dtos.CompanyDtos
{
    public class CreateCompanyDto
    {
        public string Name { get; set; }

        public string NIP { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
