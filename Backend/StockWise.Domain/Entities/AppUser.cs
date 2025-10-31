using Microsoft.AspNetCore.Identity;
namespace StockWise.Models
{
    public enum CompanyMembershipStatus { Pending, Approved, Rejected, Suspended }
    public class AppUser : IdentityUser
    {   
        public int? CompanyId { get; set; }
        public Company Company { get; set; }

        public CompanyMembershipStatus CompanyMembershipStatus { get; set; } = CompanyMembershipStatus.Pending;

    }
}
