namespace StockWise.Dtos.AccountDtos
{
    public class CreateCompanyWithAccountDto
    {
        public string UserName {  get; set; }
        public string Email {  get; set; }
        public string Password { get; set; }
        public string CompanyName { get; set; }
        public string NIP { get; set; }
        public string? CompanyEmail { get; set; } = null;
        public string? Address { get; set; } = null;
        public string? Phone { get; set; } = null;
    }
}
