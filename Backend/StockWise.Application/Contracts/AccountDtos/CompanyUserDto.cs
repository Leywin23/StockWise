using StockWise.Models;

namespace StockWise.Application.Contracts.AccountDtos
{
    public class CompanyUserDto
    {
        public string UserName { get; set; }
        public string Email {  get; set; }
        public CompanyMembershipStatus CompanyMembershipStatus { get; set; } = CompanyMembershipStatus.Pending;
    }
}
