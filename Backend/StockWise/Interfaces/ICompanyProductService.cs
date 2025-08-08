using StockWise.Dtos.CompanyDtos;
using StockWise.Dtos.CompanyProductDtos;
using StockWise.Models;

namespace StockWise.Interfaces
{
    public interface ICompanyProductService
    {
        Task<List<CompanyProduct>> GetCompanyProductsAsync(AppUser user);
        Task<CompanyProduct> GetCompanyProductAsync(AppUser user, int companyProductId);
        Task<CompanyProduct> CreateCompanyProductAsync(Company company, CreateCompanyProductDto productDto);
        Task<CompanyProduct> UpdateCompanyProductAsync(AppUser user,CompanyProduct companyProduct);
        Task<CompanyProduct> DeleteCompanyProductAsync(AppUser user,string companyProductId);
    }
}
