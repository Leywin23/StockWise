using Microsoft.AspNetCore.Mvc;
using StockWise.Dtos.CompanyDtos;
using StockWise.Models;

namespace StockWise.Interfaces
{
    public interface ICompanyService
    {
        Task<Company?> GetCompanyAsync(AppUser user);
        Task<Company?> CreateCompanyAsync(CreateCompanyDto companyDto);
        Task<Company?> UpdateCompanyAsync(UpdateCompanyDto companyDto, AppUser user);
        Task<Company?> DeleteCompanyAsync(AppUser user);
    }
}
