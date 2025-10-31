namespace StockWise.Application.Contracts.AccountDtos
{
    public class RegisterDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string CompanyNIP { get; set; }
    }
}
