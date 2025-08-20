using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace StockWise.Models
{
    public class AppUser : IdentityUser
    {   
        public int? CompanyId { get; set; }
        public Company Company { get; set; }
        public bool IsEmailConfirmed { get; set; }
    }
}
