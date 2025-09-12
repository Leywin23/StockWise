using StockWise.Dtos.CompanyDtos;
using StockWise.Dtos.CompanyProductDtos;
using StockWise.Models;
using System.Threading.Tasks;

namespace StockWise.Interfaces
{
    public interface ICompanyProductService
    {
        Task<PageResult<CompanyProduct>> GetCompanyProductsAsync(AppUser user, CompanyProductQueryParams q);
        Task<CompanyProduct?> GetCompanyProductAsync(AppUser user, int companyProductId, bool withDetails = false);
        Task<CompanyProduct> CreateCompanyProductAsync(Company company, CreateCompanyProductDto productDto);
        Task<CompanyProduct> UpdateCompanyProductAsync(int productId, AppUser user, UpdateCompanyProductDto companyProductDto);
        Task<CompanyProduct> DeleteCompanyProductAsync(AppUser user, int productId);
    }
}
