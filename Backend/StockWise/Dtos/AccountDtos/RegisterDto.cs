namespace StockWise.Dtos
{
    public class RegisterDto
    {
        public string UserName {  get; set; }
        public string Email { get; set;}
        public string Password { get; set;}
        public long CompanyNIP {  get; set;}
    }
}
