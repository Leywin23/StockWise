using StockWise.Dtos.CompanyDtos;
using StockWise.Dtos.CompanyProductDtos;
using StockWise.Helpers;
using StockWise.Models;
using System.Threading.Tasks;

namespace StockWise.Interfaces
{
    public interface ICompanyProductService
    {
        Task<ServiceResult<PageResult<CompanyProduct>>> GetCompanyProductsAsync(AppUser user, CompanyProductQueryParams q, bool withDetails = false);
        Task<ServiceResult<CompanyProduct>> GetCompanyProductAsyncById(AppUser user, int companyProductId, bool withDetails = false);
        Task<ServiceResult<CompanyProductDto>> CreateCompanyProductAsync(CreateCompanyProductDto productDto, AppUser user);
        Task<ServiceResult<CompanyProductDto>> UpdateCompanyProductAsync(int productId, AppUser user, UpdateCompanyProductDto companyProductDto);
        Task<ServiceResult<CompanyProduct>> DeleteCompanyProductAsync(AppUser user, int productId);
    }
}
