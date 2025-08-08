using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Dtos.CompanyDtos;
using StockWise.Dtos.CompanyProductDtos;
using StockWise.Dtos.ProductDtos;
using StockWise.Interfaces;
using StockWise.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace StockWise.Services
{
    public class CompanyProductService : ICompanyProductService
    {
        private readonly StockWiseDb _context;

        public CompanyProductService(StockWiseDb context)
        {
            _context = context;
        }
        public async Task<CompanyProduct> CreateCompanyProductAsync(Company company, CreateCompanyProductDto productDto)
        {
            var newCompanyProduct = new CompanyProduct
            {
                CompanyProductName = productDto.ProductName,
                EAN = productDto.EAN,
                Image = productDto.Image,
                Description = productDto.Description,
                ShoppingPrice = productDto.ShoppingPrice,
                SellingPrice = productDto.SellingPrice,
                Stock = productDto.Stock,
                Company = company,
                IsAvailableForOrder = productDto.IsAvailableForOrder
            };

            await _context.CompanyProducts.AddAsync(newCompanyProduct);
            await _context.SaveChangesAsync();

            return newCompanyProduct;
        }

        public async Task<CompanyProduct?> DeleteCompanyProductAsync(AppUser user, int productId)
        {
            var company = user.Company;

            if (company == null)
                return null;

            var productToDelete = await _context.CompanyProducts
                .FirstOrDefaultAsync(cp => cp.Company.Id == company.Id && cp.CompanyProductId == productId);

            if (productToDelete == null)
                return null;

            _context.CompanyProducts.Remove(productToDelete);
            await _context.SaveChangesAsync();

            return productToDelete;
        }

        public async Task<CompanyProduct> GetCompanyProductAsync(AppUser user,int companyProductId)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == user.Company.Id);

            var product = await _context.CompanyProducts.FirstOrDefaultAsync(cp => cp.Company.Id == company.Id);

            return product;
        }

        public async Task<List<CompanyProduct>> GetCompanyProductsAsync(AppUser user)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == user.Company.Id);

            var products = await _context.CompanyProducts.Where(cp => cp.Company.Id == company.Id).ToListAsync();

            return products;
        }


        public Task<CompanyProduct> UpdateCompanyProductAsync(AppUser user, CompanyProduct companyProduct)
        {
            throw new NotImplementedException();
        }
    }
}
