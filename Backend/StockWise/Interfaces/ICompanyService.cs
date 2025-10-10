using Microsoft.AspNetCore.Mvc;
using StockWise.Dtos.CompanyDtos;
using StockWise.Helpers;
using StockWise.Models;
using System.Threading.Tasks;

namespace StockWise.Interfaces
{
    public interface ICompanyService
    {
        Task<ServiceResult<CompanyDto>> GetCompanyAsync(AppUser user);
        Task<ServiceResult<PageResult<Company>>> GetAllCompanyAsync(CompanyQueryParams q);
        Task<ServiceResult<AdvancedCompanyDto>> GetAdvancedCompanyDataAsync(AppUser user);
        Task<ServiceResult<CompanyDto>> CreateCompanyAsync(CreateCompanyDto companyDto);
        Task<ServiceResult<CompanyDto>> UpdateCompanyAsync(UpdateCompanyDto companyDto, AppUser user);
        Task<ServiceResult<CompanyDto>> DeleteCompanyAsync(AppUser user);
    }
}
