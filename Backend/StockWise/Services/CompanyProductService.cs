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
            var Price = Money.Of(productDto.Price, productDto.Currency.Code);

            var newCompanyProduct = new CompanyProduct
            {
                CompanyProductName = productDto.ProductName,
                EAN = productDto.EAN,
                Image = productDto.Image,
                Description = productDto.Description,
                Price = Price,
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


        public async Task<CompanyProduct?> GetCompanyProductAsync(AppUser user,int companyProductId, bool withDetails = false)
        {
            if (user?.Company == null) return null;

            var companyId = user.CompanyId;

            IQueryable<CompanyProduct> query = _context.CompanyProducts.AsNoTracking()
                .Where(cp=>cp.CompanyProductId == companyProductId && cp.CompanyId == companyId);

            if (withDetails)
            {
                query = query
                    .Include(cp => cp.Company)
                    .Include(cp => cp.InventoryMovements);
            }

            var result = await query.FirstOrDefaultAsync();
            if (result == null) return null;

            return result;
        }

        public async Task<PageResult<CompanyProduct>> GetCompanyProductsAsync(
        AppUser user,
        CompanyProductQueryParams q,
        bool withDetails = false)
        {
            if (q.Page <= 0) q.Page = 1;
            if (q.PageSize <= 0) q.PageSize = 10;
            if (q.PageSize > 100) q.PageSize = 100;

            if (q.MinTotal.HasValue && q.MaxTotal.HasValue && q.MinTotal > q.MaxTotal)
                throw new ArgumentException("MinTotal cannot be > MaxTotal");

            if (user?.CompanyId == null)
                return new PageResult<CompanyProduct> { Page = q.Page, PageSize = q.PageSize, TotalCount = 0, Items = new() };

            var companyId = user.CompanyId.Value;

            IQueryable<CompanyProduct> query = _context.CompanyProducts
                .AsNoTracking()
                .Where(cp => cp.CompanyId == companyId);

            if (q.Stock > 0)
                query = query.Where(cp => cp.Stock >= q.Stock);

            if (q.IsAvailableForOrder)
                query = query.Where(cp => cp.IsAvailableForOrder == true);

            if (q.MinTotal.HasValue && q.MinTotal.Value > 0)
                query = query.Where(cp => cp.Price.Amount >= q.MinTotal.Value);

            if (q.MaxTotal.HasValue && q.MaxTotal.Value > 0)
                query = query.Where(cp => cp.Price.Amount <= q.MaxTotal.Value);

            if (withDetails)
            {
                query = query
                    .Include(cp => cp.Company)
                    .Include(cp => cp.InventoryMovements);
            }

            var key = (q.SortedBy ?? "stock").Trim().ToLowerInvariant();

            IOrderedQueryable<CompanyProduct>? ordered = key switch
            {
                "id" => q.SortDir == SortDir.Asc ? query.OrderBy(cp => cp.CompanyProductId)
                                                    : query.OrderByDescending(cp => cp.CompanyProductId),

                "stock" => q.SortDir == SortDir.Asc ? query.OrderBy(cp => cp.Stock)
                                                    : query.OrderByDescending(cp => cp.Stock),

                "price" => q.SortDir == SortDir.Asc ? query.OrderBy(cp => cp.Price.Amount)
                                                    : query.OrderByDescending(cp => cp.Price.Amount),

                _ => null
            };

            if (ordered is null)
                throw new ArgumentException($"Unknown SortedBy: {q.SortedBy}. Allowed: id, stock, price");

            ordered = q.SortDir == SortDir.Asc
                ? ordered.ThenBy(cp => cp.CompanyProductId)
                : ordered.ThenByDescending(cp => cp.CompanyProductId);

            var totalCount = await ordered.CountAsync();

            var items = await ordered
                .Skip((q.Page - 1) * q.PageSize)
                .Take(q.PageSize)
                .ToListAsync();

            return new PageResult<CompanyProduct>
            {
                Page = q.Page,
                PageSize = q.PageSize,
                TotalCount = totalCount,
                SortBy = key,     
                SortDir = q.SortDir,
                Items = items
            };
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

            var Price = Money.Of(companyProductDto.Price, companyProductDto.Currency.Code);

            product.CompanyProductName = companyProductDto.CompanyProductName;
            product.EAN = companyProductDto.Ean;
            product.Image = companyProductDto.Image;
            product.Description = companyProductDto.Description;
            product.Price = Price;
            product.Stock = companyProductDto.Stock;
            product.IsAvailableForOrder = companyProductDto.IsAvailableForOrder;

            await _context.SaveChangesAsync();

            return product;
        }
    }
}
