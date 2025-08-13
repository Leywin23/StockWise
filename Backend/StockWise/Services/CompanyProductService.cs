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


        public async Task<CompanyProduct> UpdateCompanyProductAsync(int productId, AppUser user, UpdateCompanyProductDto companyProductDto)
        {
            if(user.Company == null)
            {
                return null;
            }

            var company = await _context.Companies.Include(c => c.CompanyProducts).FirstOrDefaultAsync(c => c.Id == user.CompanyId);
            if (company == null) {
                return null;
            }

            var product = company.CompanyProducts.FirstOrDefault(cp => cp.CompanyProductId == productId);
            if (product == null)
                return null;

            var duplicate = company.CompanyProducts.Any(cp => cp.CompanyProductId != productId &&
           (cp.EAN == companyProductDto.Ean || cp.CompanyProductName == companyProductDto.CompanyProductName));

            if (duplicate)
                throw new InvalidOperationException("Another product with the same EAN or name already exists in your company.");

            product.CompanyProductName = companyProductDto.CompanyProductName;
            product.EAN = companyProductDto.Ean;
            product.Image = companyProductDto.Image;
            product.Description = companyProductDto.Description;
            product.ShoppingPrice = companyProductDto.ShoppingPrice;
            product.SellingPrice = companyProductDto.SellingPrice;
            product.Stock = companyProductDto.Stock;
            product.IsAvailableForOrder = companyProductDto.IsAvailableForOrder;

            await _context.SaveChangesAsync();

            return product;
        }
    }
}
