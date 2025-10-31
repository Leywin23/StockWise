using System.Threading.Tasks;
using StockWise.Application.Abstractions;                
using StockWise.Application.Contracts.CompanyDtos;       
using StockWise.Models;
namespace StockWise.Application.Interfaces
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
