using AutoMapper;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockWise.Application.Abstractions;
using StockWise.Application.Contracts.CompanyProductDtos;
using StockWise.Application.Contracts.InventoryMovementDtos;
using StockWise.Application.Interfaces;
using StockWise.Infrastructure.Persistence;
using StockWise.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace StockWise.Infrastructure.Services
{
    public class CompanyProductService : ICompanyProductService
    {
        private readonly StockWiseDb _context;
        private readonly IMapper _mapper;
        private readonly IInventoryMovementService _inventoryMovementService;
        private readonly BlobStorageService _blobStorage;
        private readonly BlobServiceClient _blob;
        private readonly IOptionsSnapshot<AzureStorageOptions> _opts;
        private readonly ILogger<CompanyProductService> _log;

        public CompanyProductService(StockWiseDb context, IMapper mapper, IInventoryMovementService inventoryMovementService, BlobStorageService blobStorage, BlobServiceClient blob, IOptionsSnapshot<AzureStorageOptions> opts, ILogger<CompanyProductService> log)
        {
            _context = context;
            _mapper = mapper;
            _inventoryMovementService = inventoryMovementService;
            _blobStorage = blobStorage;
            _blob = blob;
            _opts = opts;
            _log = log;
        }
        public async Task<ServiceResult<PageResult<CompanyProduct>>> GetCompanyProductsAsync(
        AppUser user,
        CompanyProductQueryParams q,
        bool withDetails = false)
        {
            if (user.CompanyMembershipStatus != CompanyMembershipStatus.Approved)
            {
                return ServiceResult<PageResult<CompanyProduct>>.Unauthorized("You have to be approved by a manager to use this functionality");
            }
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
                .Where(cp => cp.CompanyId == companyId)
                .Include(cp => cp.Category)
                .Include(cp => cp.InventoryMovements);

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
        public async Task<ServiceResult<CompanyProductDto>> GetCompanyProductAsyncById(AppUser user, int companyProductId, bool withDetails=true)
        {
            if (user?.Company == null) return ServiceResult<CompanyProductDto>.Forbidden("User is not assigned to any company.");

            if (user.CompanyMembershipStatus != CompanyMembershipStatus.Approved)
            {
                return ServiceResult<CompanyProductDto>.Forbidden("You have to be approved by a manager to use this functionality");
            }
            var companyId = user.CompanyId.Value;

            IQueryable<CompanyProduct> query = _context.CompanyProducts
                .AsNoTracking()
                .Where(cp => cp.CompanyProductId == companyProductId && cp.CompanyId == companyId)
                .Include(cp=>cp.Category);

            if (withDetails)
            {
                query = query
                    .Include(cp => cp.Company)
                    .Include(cp => cp.InventoryMovements);
            }

            var product = await query.FirstOrDefaultAsync();

            if (product == null)
                return ServiceResult<CompanyProductDto>.NotFound("Product not found.");

            var dto = _mapper.Map<CompanyProductDto>(product);
            return ServiceResult<CompanyProductDto>.Ok(dto);
        }

        public async Task<ServiceResult<CompanyProductDto>> CreateCompanyProductAsync(CreateCompanyProductDto dto, AppUser user, CancellationToken ct = default)
        {
            if (user.CompanyMembershipStatus != CompanyMembershipStatus.Approved)
                return ServiceResult<CompanyProductDto>.Forbidden("You have to be approved by a manager to use this functionality");

            dto.CompanyProductName = dto.CompanyProductName?.Trim();
            dto.EAN = dto.EAN?.Trim();
            dto.Category = dto.Category?.Trim();
            dto.Currency = dto.Currency?.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(dto.CompanyProductName))
                return ServiceResult<CompanyProductDto>.BadRequest("CompanyProductName is required.");
            if (string.IsNullOrWhiteSpace(dto.EAN))
                return ServiceResult<CompanyProductDto>.BadRequest("EAN is required.");
            if (string.IsNullOrWhiteSpace(dto.Currency))
                return ServiceResult<CompanyProductDto>.BadRequest("Currency code is required.");
            if (dto.Currency.Length != 3)
                return ServiceResult<CompanyProductDto>.BadRequest("Currency code must be a 3-letter ISO code (e.g., PLN, EUR).");
            if (dto.Price < 0)
                return ServiceResult<CompanyProductDto>.BadRequest("Price must be >= 0.");
            if (user.CompanyId is null)
                return ServiceResult<CompanyProductDto>.BadRequest("User has no assigned company.");

            var companyId = user.CompanyId.Value;

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == dto.Category, ct);
            if (category is null)
            {
                category = new Category { Name = dto.Category! };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync(ct);
            }

            if (await _context.CompanyProducts.AnyAsync(p => p.CompanyId == companyId && p.EAN == dto.EAN && p.IsDeleted != true, ct))
                return ServiceResult<CompanyProductDto>.Conflict($"Product with EAN {dto.EAN} already exists for this company.");

            if (await _context.CompanyProducts.AnyAsync(p => p.CompanyId == companyId && p.CompanyProductName == dto.CompanyProductName && p.IsDeleted != true, ct))
                return ServiceResult<CompanyProductDto>.Conflict($"Product named '{dto.CompanyProductName}' already exists for this company.");

            string? imageUrl = null;
            string? uploadedBlobName = null;

            if (dto.ImageFile is not null && dto.ImageFile.Length > 0)
            {
                if (!dto.ImageFile.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                    return ServiceResult<CompanyProductDto>.BadRequest("Only image files are allowed.");
                const long MAX = 5 * 1024 * 1024; 
                if (dto.ImageFile.Length > MAX)
                    return ServiceResult<CompanyProductDto>.BadRequest("Image too large (max 5 MB).");

                var ext = Path.GetExtension(dto.ImageFile.FileName);
                var safeName = $"{Guid.NewGuid()}{ext}".ToLowerInvariant();
                uploadedBlobName = safeName;

                await using var s = dto.ImageFile.OpenReadStream();
                var url = await _blobStorage.UploadAsync(s, safeName, dto.ImageFile.ContentType, ct);
                if (string.IsNullOrWhiteSpace(url))
                    return ServiceResult<CompanyProductDto>.ServerError("Image upload failed.");
                imageUrl = url;
            }

            var product = _mapper.Map<CompanyProduct>(dto); 
            product.CompanyId = companyId;
            product.CategoryId = category.CategoryId;
            product.Image = imageUrl;
            product.Stock = 0;
            product.InventoryMovements = new List<InventoryMovement>();

            product.Price = Money.Of(dto.Price, dto.Currency);

            await using var tx = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                _context.CompanyProducts.Add(product);
                await _context.SaveChangesAsync(ct);

                if (dto.Stock > 0)
                {
                    var movementDto = new InventoryMovementDto
                    {
                        Date = DateTime.UtcNow,
                        Type = MovementType.Inbound,
                        CompanyProductId = product.CompanyProductId,
                        Quantity = dto.Stock,
                    };

                    var movementResult = await _inventoryMovementService.AddMovementAsync(movementDto); 
                    if (movementResult is null || !movementResult.IsSuccess)
                    {
                        await tx.RollbackAsync(ct);


                        var errMsg = movementResult?.Error.ToString() ?? "Unknown error";
                        return ServiceResult<CompanyProductDto>.BadRequest($"Failed to create inventory movement: {errMsg}");
                    }
                }

                await tx.CommitAsync(ct);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }

            var productDto = _mapper.Map<CompanyProductDto>(product);
            return ServiceResult<CompanyProductDto>.Ok(productDto);
        }

        public async Task<ServiceResult<CompanyProductDto>> DeleteCompanyProductAsync(
        AppUser user,
        int productId,
        CancellationToken ct = default)
        {
            if (user.CompanyMembershipStatus != CompanyMembershipStatus.Approved)
                return ServiceResult<CompanyProductDto>.Forbidden(
                    "You have to be approved by a manager to use this functionality.");

            if (user.CompanyId is null)
                return ServiceResult<CompanyProductDto>.Forbidden(
                    "User is not assigned to any company.");

            var companyId = user.CompanyId.Value;

            var product = await _context.CompanyProducts
                .FirstOrDefaultAsync(cp => cp.CompanyId == companyId && cp.CompanyProductId == productId, ct);

            if (product is null)
                return ServiceResult<CompanyProductDto>.NotFound("Product not found.");

            var imageUrl = product.Image;
            var imageExist = !string.IsNullOrEmpty(imageUrl);

            if (imageExist)
            {
                try
                {
                    await _blobStorage.DeleteAsync(imageUrl);
                    
                }
                catch (Exception ex)
                {
                    throw new Exception("Błąd podczas usuwania bloba", ex);
                }
            }

            product.IsDeleted = true;
            product.IsAvailableForOrder = false;
            product.CompanyProductName = "Deleted";
            product.EAN = "Deleted";
            product.Description = "Deleted";

            _context.CompanyProducts.Update(product);
            await _context.SaveChangesAsync(ct);

            var productDto = _mapper.Map<CompanyProductDto>(product);
            return ServiceResult<CompanyProductDto>.Ok(productDto);
        }


        public async Task<ServiceResult<CompanyProductDto>> UpdateCompanyProductAsync(int productId, AppUser user, UpdateCompanyProductDto companyProductDto, CancellationToken ct= default)
        {
            if (user.CompanyMembershipStatus != CompanyMembershipStatus.Approved)
            {
                return ServiceResult<CompanyProductDto>.Forbidden("You have to be approved by a manager to use this functionality");
            }

            var company = await _context.Companies.Include(c => c.CompanyProducts).FirstOrDefaultAsync(c => c.Id == user.CompanyId);
            if (company == null) {
                return ServiceResult<CompanyProductDto>.NotFound("User isn't assigned to any company");
            }

            var product = company.CompanyProducts.FirstOrDefault(cp => cp.CompanyProductId == productId);
            if (product == null)
                return ServiceResult<CompanyProductDto>.NotFound($"Product with id: {productId} not found");

            var Price = Money.Of(companyProductDto.Price, companyProductDto.Currency);

            if (companyProductDto.ImageFile is not null && !companyProductDto.ImageFile.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResult<CompanyProductDto>.BadRequest("Only image files are allowed.");
            }

            string? imageUrl = null;
            string? uploadedBlobName = null;

            if (companyProductDto.ImageFile is not null && companyProductDto.ImageFile.Length > 0)
            {
                if (!companyProductDto.ImageFile.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                    return ServiceResult<CompanyProductDto>.BadRequest("Only image files are allowed.");
                const long MAX = 5 * 1024 * 1024;
                if (companyProductDto.ImageFile.Length > MAX)
                    return ServiceResult<CompanyProductDto>.BadRequest("Image too large (max 5 MB).");

                var ext = Path.GetExtension(companyProductDto.ImageFile.FileName);
                var safeName = $"{Guid.NewGuid()}{ext}".ToLowerInvariant();
                uploadedBlobName = safeName;

                await using var s = companyProductDto.ImageFile.OpenReadStream();
                var url = await _blobStorage.UploadAsync(s, safeName, companyProductDto.ImageFile.ContentType, ct);
                if (string.IsNullOrWhiteSpace(url))
                    return ServiceResult<CompanyProductDto>.ServerError("Image upload failed.");
                imageUrl = url;
            }

            if (!string.IsNullOrEmpty(product.Image))
            {
                await _blobStorage.DeleteAsync(product.Image);
            }

            product.CompanyProductName = companyProductDto.CompanyProductName;
            product.Image = imageUrl;
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
