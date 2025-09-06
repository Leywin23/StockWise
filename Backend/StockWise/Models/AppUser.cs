using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace StockWise.Models
{
    public enum CompanyMembershipStatus { Pending, Approved, Rejected}
    public class AppUser : IdentityUser
    {   
        public int? CompanyId { get; set; }
        public Company Company { get; set; }

        public CompanyMembershipStatus CompanyMembershipStatus { get; set; } = CompanyMembershipStatus.Pending;

    }
}
