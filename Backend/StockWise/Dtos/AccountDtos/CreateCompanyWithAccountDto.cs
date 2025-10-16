namespace StockWise.Dtos.AccountDtos
{
    public class CreateCompanyWithAccountDto
    {
        public string UserName {  get; set; }
        public string Email {  get; set; }
        public string Password { get; set; }
        public string CompanyName { get; set; }
        public long NIP { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
    }
}
