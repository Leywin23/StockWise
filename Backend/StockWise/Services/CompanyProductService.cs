using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Dtos.CompanyDtos;
using StockWise.Dtos.CompanyProductDtos;
using StockWise.Dtos.ProductDtos;
using StockWise.Helpers;
using StockWise.Interfaces;
using StockWise.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace StockWise.Services
{
    public class CompanyProductService : ICompanyProductService
    {
        private readonly StockWiseDb _context;
        private readonly IMapper _mapper;

        public CompanyProductService(StockWiseDb context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<ServiceResult<PageResult<CompanyProduct>>> GetCompanyProductsAsync(
        AppUser user,
        CompanyProductQueryParams q,
        bool withDetails = false)
        {
            if (q.Page <= 0) q.Page = 1;
            if (q.PageSize <= 0) q.PageSize = 10;
            if (q.PageSize > 100) q.PageSize = 100;

            if (q.MinTotal.HasValue && q.MaxTotal.HasValue && q.MinTotal > q.MaxTotal)
                return ServiceResult<PageResult<CompanyProduct>>.BadRequest(
                    "Validation Failed",
                    new Dictionary<string, string[]>
                    {
                        ["minTotal"] = new[] { "MinTotal cannot be greater than MaxTotal" },
                        ["maxTotal"] = new[] { "MinTotal cannot be greater than MaxTotal." },
                    }
                );


            if (user?.CompanyId == null)
                return ServiceResult<PageResult<CompanyProduct>>.Forbidden("User is not assigned to any company.");

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
            {
                return ServiceResult<PageResult<CompanyProduct>>.BadRequest(
                    "Validation Failed",
                    new Dictionary<string, string[]>
                    {
                        ["sortedBy"] = new[] { "Unknow SortedBy. Allowed: id, stock, price" }
                    });
            }

            ordered = q.SortDir == SortDir.Asc
                ? ordered.ThenBy(cp => cp.CompanyProductId)
                : ordered.ThenByDescending(cp => cp.CompanyProductId);

            var totalCount = await ordered.CountAsync();

            var items = await ordered
                .Skip((q.Page - 1) * q.PageSize)
                .Take(q.PageSize)
                .ToListAsync();

            var page = new PageResult<CompanyProduct>
            {
                Page = q.Page,
                PageSize = q.PageSize,
                TotalCount = totalCount,
                SortBy = key,
                SortDir = q.SortDir,
                Items = items
            };

            return ServiceResult<PageResult<CompanyProduct>>.Ok(page);
        }
        public async Task<ServiceResult<CompanyProduct>> GetCompanyProductAsyncById(AppUser user, int companyProductId, bool withDetails = false)
        {
            if (user?.Company == null) return ServiceResult<CompanyProduct>.Forbidden("User is not assigned to any company.");

            var companyId = user.CompanyId.Value;

            IQueryable<CompanyProduct> query = _context.CompanyProducts
                .AsNoTracking()
                .Where(cp => cp.CompanyProductId == companyProductId && cp.CompanyId == companyId);

            if (withDetails)
            {
                query = query
                    .Include(cp => cp.Company)
                    .Include(cp => cp.InventoryMovements);
            }

            var product = await query.FirstOrDefaultAsync();

            if (product == null)
                return ServiceResult<CompanyProduct>.NotFound("Product not found.");

            return ServiceResult<CompanyProduct>.Ok(product);
        }

        public async Task<ServiceResult<CompanyProductDto>> CreateCompanyProductAsync(CreateCompanyProductDto dto, AppUser user)
        {
            dto.CompanyProductName = dto.CompanyProductName?.Trim();
            dto.EAN = dto.EAN?.Trim();
            dto.Category = dto.Category?.Trim();

            if (string.IsNullOrWhiteSpace(dto.CompanyProductName))
                return ServiceResult<CompanyProductDto>.BadRequest("CompanyProductName is required.");
            if (string.IsNullOrWhiteSpace(dto.EAN))
                return ServiceResult<CompanyProductDto>.BadRequest("EAN is required.");
            if (dto.Currency?.Code is null)
                return ServiceResult<CompanyProductDto>.BadRequest("Currency code is required.");

            if (user.CompanyId is null)
                return ServiceResult<CompanyProductDto>.BadRequest("User has no assigned company.");

            int companyId = user.CompanyId.Value;

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == dto.Category);
            if (category == null)
            {
                category = new Category { Name = dto.Category };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
            }

            bool existsByEan = await _context.CompanyProducts
                .AnyAsync(p => p.CompanyId == companyId && p.EAN == dto.EAN);
            if (existsByEan)
                return ServiceResult<CompanyProductDto>.Conflict($"Product with EAN {dto.EAN} already exists for this company.");

            bool existsByName = await _context.CompanyProducts
                .AnyAsync(p => p.CompanyId == companyId && p.CompanyProductName == dto.CompanyProductName);
            if (existsByName)
                return ServiceResult<CompanyProductDto>.Conflict($"Product named '{dto.CompanyProductName}' already exists for this company.");

            var product = _mapper.Map<CompanyProduct>(dto);
            product.CompanyId = companyId;
            product.CategoryId = category.CategoryId;

            _context.CompanyProducts.Add(product);
            await _context.SaveChangesAsync();


            var productDto = _mapper.Map<CompanyProductDto>(product);
            return ServiceResult<CompanyProductDto>.Ok(productDto);
        }

        public async Task<ServiceResult<CompanyProduct>> DeleteCompanyProductAsync(AppUser user, int productId)
        {
            if (user?.CompanyId == null)
                return ServiceResult<CompanyProduct>.Forbidden("User is not assigned to any company.");

            var companyId = user.CompanyId.Value;

            var productToDelete = await _context.CompanyProducts
                .FirstOrDefaultAsync(cp => cp.CompanyId == companyId && cp.CompanyProductId == productId);

            if (productToDelete == null)
                return ServiceResult<CompanyProduct>.NotFound("Product not found.");

            _context.CompanyProducts.Remove(productToDelete);
            await _context.SaveChangesAsync();

            return ServiceResult<CompanyProduct>.Ok(productToDelete);
        }


        public async Task<ServiceResult<CompanyProductDto>> UpdateCompanyProductAsync(int productId, AppUser user, UpdateCompanyProductDto companyProductDto)
        {
            var company = await _context.Companies.Include(c => c.CompanyProducts).FirstOrDefaultAsync(c => c.Id == user.CompanyId);
            if (company == null) {
                return ServiceResult<CompanyProductDto>.NotFound("User isn't assigned to any company");
            }

            var product = company.CompanyProducts.FirstOrDefault(cp => cp.CompanyProductId == productId);
            if (product == null)
                return ServiceResult<CompanyProductDto>.NotFound($"Product with id: {productId} not found");

            var duplicate = company.CompanyProducts.Any(cp => cp.CompanyProductId != productId &&
           (cp.EAN == companyProductDto.Ean || cp.CompanyProductName == companyProductDto.CompanyProductName));

            if (duplicate)
                return ServiceResult<CompanyProductDto>.Conflict("Another product with the same name or EAN already exists.");

            var Price = Money.Of(companyProductDto.Price, companyProductDto.Currency.Code);

            product.CompanyProductName = companyProductDto.CompanyProductName;
            product.EAN = companyProductDto.Ean;
            product.Image = companyProductDto.Image;
            product.Description = companyProductDto.Description;
            product.Price = Price;
            product.Stock = companyProductDto.Stock;
            product.IsAvailableForOrder = companyProductDto.IsAvailableForOrder;

            await _context.SaveChangesAsync();

            var productDto = _mapper.Map<CompanyProductDto>(product);

            return ServiceResult<CompanyProductDto>.Ok(productDto);
        }
    }
}
