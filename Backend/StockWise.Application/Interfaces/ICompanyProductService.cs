using StockWise.Application.Abstractions;
using StockWise.Application.Contracts.CompanyProductDtos;
using StockWise.Models;
using System.Threading.Tasks;

namespace StockWise.Application.Interfaces
{
    public interface ICompanyProductService
    {
        Task<ServiceResult<PageResult<CompanyProduct>>> GetCompanyProductsAsync(AppUser user, CompanyProductQueryParams q, bool withDetails = true);
        Task<ServiceResult<CompanyProductDto>> GetCompanyProductAsyncById(AppUser user, int companyProductId, bool withDetails = false);
        Task<ServiceResult<PageResult<CompanyProductDtoWithCompany>>> GetAllAvailableCompanyProducts(AppUser user, CompanyProductsAvailableQueryParams q, CancellationToken ct = default);
        Task<ServiceResult<CompanyProductDto>> CreateCompanyProductAsync(CreateCompanyProductDto productDto, AppUser user, CancellationToken ct = default);
        Task<ServiceResult<CompanyProductDto>> UpdateCompanyProductAsync(int productId, AppUser user, UpdateCompanyProductDto companyProductDto, CancellationToken ct = default);
        Task<ServiceResult<CompanyProductDto>> DeleteCompanyProductAsync(AppUser user, int productId, CancellationToken ct = default);
    }
}
