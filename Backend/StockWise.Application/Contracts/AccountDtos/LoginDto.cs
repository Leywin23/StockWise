namespace StockWise.Application.Contracts.AccountDtos
{
    public class LoginDto
    {
        public string UserName {  get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Token {  get; set; }
    }
}
