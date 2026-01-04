using StockWise.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Application.Contracts.AccountDtos
{
    public class WorkerDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email {  get; set; }
        public string Role { get; set; }
        public CompanyMembershipStatus CompanyMembershipStatus { get; set; } = CompanyMembershipStatus.Pending;
    }
}
