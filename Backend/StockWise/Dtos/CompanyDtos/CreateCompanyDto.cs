namespace StockWise.Dtos.CompanyDtos
{
    public class CreateCompanyDto
    {
        public string Name { get; set; }
        public long NIP { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
