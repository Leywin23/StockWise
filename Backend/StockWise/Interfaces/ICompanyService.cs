using Microsoft.AspNetCore.Mvc;
using StockWise.Dtos.CompanyDtos;
using StockWise.Models;

namespace StockWise.Interfaces
{
    public interface ICompanyService
    {
        Task<Company?> GetCompanyAsync(string companyId, AppUser user);
        Task<Company> CreateComapnyAsync(CreateCompanyDto companyDto);
        Task<Company?> UpdateComapnyAsync(UpdateCompanyDto companyDto, AppUser user);
        Task? DeleteCompanyAsync(string companyId, AppUser user);
    }
}
