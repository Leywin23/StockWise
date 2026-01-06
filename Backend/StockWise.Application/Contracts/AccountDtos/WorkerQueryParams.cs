using StockWise.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Application.Contracts.AccountDtos
{
    public enum CompanyRole
    {
        Manager,
        Worker,
    }
    public enum WorkersSortBy
    {
        Name,
        Email,
        Role,
        CompanyMembershipStatus,
    }
    public class WorkerQueryParams
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public CompanyRole Role { get; set; } = CompanyRole.Manager;
        public CompanyMembershipStatus CompanyMembershipStatus { get; set; } = CompanyMembershipStatus.Approved;
        public WorkersSortBy SortedBy { get; set; } = WorkersSortBy.Role;
        public SortDir SortDir { get; set; } = SortDir.Asc;

    }
}
