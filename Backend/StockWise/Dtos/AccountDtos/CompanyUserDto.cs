using StockWise.Models;

namespace StockWise.Dtos.AccountDtos
{
    public class CompanyUserDto
    {
        public String UserName { get; set; }
        public string Email {  get; set; }
        public CompanyMembershipStatus CompanyMembershipStatus { get; set; } = CompanyMembershipStatus.Pending;
    }
}
