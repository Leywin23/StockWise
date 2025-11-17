using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockWise.Application.Abstractions;
using StockWise.Application.Contracts.ProductDtos;
using StockWise.Application.Interfaces;
using StockWise.Infrastructure.Persistence;
using StockWise.Models;
namespace StockWise.Infrastructure.Services
{
    public class ProductService: IProductService
    {
        private readonly StockWiseDb _context;
        private readonly IMapper _mapper;
        private readonly BlobStorageService _blobStorage;
        public ProductService(StockWiseDb context, IMapper mapper, BlobStorageService blobStorage)
        {
            _context = context;
            _mapper = mapper;
            _blobStorage = blobStorage;
        }
        public async Task<ServiceResult<ProductDto>> GetProductById(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .ThenInclude(c => c.Parent)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return ServiceResult<ProductDto>.NotFound($"Product with id: {id} not found");
            }

            var categoryFullPath = GetCategoryFullPath(product.Category);

            var result = new ProductDto
            {
                ProductName = product.ProductName,
                Ean = product.EAN,
                Description = product.Description,
                SellingPrice = product.SellingPrice.Amount,
                ShoppingPrice = product.ShoppingPrice.Amount,
                Currency = product.SellingPrice.Currency,
                CategoryString = GetCategoryFullPath(product.Category),
            };

            return ServiceResult<ProductDto>.Ok(result);
        }

        public async Task<ServiceResult<List<ProductDto>>> GetProducts()
        {
            var products = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category).ThenInclude(c => c.Parent)
                .ToListAsync();

            var result = products.Select(p => new ProductDto
                {
                    ProductName = p.ProductName,
                    Ean = p.EAN,
                    Description = p.Description,
                    SellingPrice = p.SellingPrice.Amount,
                    ShoppingPrice = p.ShoppingPrice.Amount,
                    Currency = p.SellingPrice.Currency,
                    CategoryString = GetCategoryFullPath(p.Category),
                }
            ).ToList();

            return ServiceResult<List<ProductDto>>.Ok(result);
        }

        public async Task<ServiceResult<Product>> AddProduct(CreateProductDto productDto, CancellationToken ct = default)
        {
            if (productDto is null)
                return ServiceResult<Product>.BadRequest("Product data is required.");

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == productDto.Category, ct);

            if (category is null)
            {
                category = new Category { Name = productDto.Category };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync(ct);
            }

            var exist = await _context.Products.FirstOrDefaultAsync(p => p.EAN == productDto.EAN, ct);
            if (exist != null)
                return ServiceResult<Product>.BadRequest($"Product with EAN {exist.EAN} already added");

            string? imageUrl = null;

            if (productDto.Image is not null && productDto.Image.Length > 0)
            {
                if (!productDto.Image.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                    return ServiceResult<Product>.BadRequest("Only image files are allowed");

                const long MAX = 5 * 1024 * 1024;
                if (productDto.Image.Length > MAX)
                    return ServiceResult<Product>.BadRequest("Image too large (max 5 MB).");

                var ext = Path.GetExtension(productDto.Image.FileName);

                var safeName = $"{Guid.NewGuid()}{ext}".ToLowerInvariant();

                await using var s = productDto.Image.OpenReadStream();
                var url = await _blobStorage.UploadAsync(s, safeName, productDto.Image.ContentType, ct);
                if (string.IsNullOrWhiteSpace(url))
                    return ServiceResult<Product>.ServerError("Image upload failed.");

                imageUrl = url;
            }

            var product = _mapper.Map<Product>(productDto);   
            product.Image = imageUrl;
            product.CategoryId = category.CategoryId;                
            await _context.Products.AddAsync(product, ct);
            await _context.SaveChangesAsync(ct);

            return ServiceResult<Product>.Ok(product);
        }

        public async Task<ServiceResult<Product>> DeleteProduct(int id, CancellationToken ct = default)
        {
            var productToDelete = await _context.Products.FindAsync(id);
            if (productToDelete == null)
            {
                return ServiceResult<Product>.NotFound($"Couldn't find a product with given id: {id}");
            }
            if(productToDelete.Image != null)
            {
                await _blobStorage.DeleteAsync(productToDelete.Image);
            }
            _context.Products.Remove(productToDelete);
            await _context.SaveChangesAsync();
            var productToDeleteDto = _mapper.Map<Product>(productToDelete);
            return ServiceResult<Product>.Ok(productToDelete);
        }

        public async Task<ServiceResult<Product>> UpdateProduct(int productId ,UpdateProductDto productDto, CancellationToken ct = default)
        {
            var productToUpdate = await _context.Products.FirstOrDefaultAsync(x => x.ProductId == productId);
            if (productToUpdate == null)
                return ServiceResult<Product>.NotFound("Couldn't find a product");

            var category = await _context.Categories.FirstOrDefaultAsync(x => x.Name == productDto.CategoryName);
            if (category == null)
            {
                return ServiceResult<Product>.NotFound("Coundn't find a category with this name");
            }
            if(productToUpdate.Image != null)
            {
                await _blobStorage.DeleteAsync(productToUpdate.Image);
            }

            string? imageUrl = null;

            if(productDto.Image != null && productDto.Image.Length > 0)
            {
                if (!productDto.Image.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                    return ServiceResult<Product>.BadRequest("Only image file are allowed");
                const long MAX = 5 * 1024 * 1024;
                if (productDto.Image.Length > MAX)
                    return ServiceResult<Product>.BadRequest("Image too large (max 5 MB).");
                var s = productDto.Image.OpenReadStream();
                var ext = Path.GetExtension(productDto.Image.FileName);
                var safeName = $"{Guid.NewGuid()}{ext}".ToLowerInvariant();
                var url = await _blobStorage.UploadAsync(s,safeName, productDto.Image.ContentType, ct);

                imageUrl = url;
            }
            var shoppingPrice = Money.Of(productDto.ShoppingPrice, productDto.Currency);
            var sellingPrice = Money.Of(productDto.SellingPrice, productDto.Currency);

            productToUpdate.ProductName = productDto.ProductName;
            productToUpdate.CategoryId = category.CategoryId;
            productToUpdate.Description = productDto.Description;
            productToUpdate.ShoppingPrice = shoppingPrice;
            productToUpdate.SellingPrice = sellingPrice;
            productToUpdate.Image = imageUrl;

            _context.Products.Update(productToUpdate);
            await _context.SaveChangesAsync();
            return ServiceResult<Product>.Ok(productToUpdate);
        }

        public string GetCategoryFullPath(Category category)
        {
            var names = new List<string>();
            var current = category;

            while (current != null)
            {
                names.Add(current.Name);
                if (current.Parent == null && current.ParentId != null)
                {
                    current = _context.Categories.FirstOrDefault(c => c.CategoryId == current.ParentId);
                }
                else
                {
                    current = current.Parent;
                }
            }
            names.Reverse();
            return string.Join(" > ", names);
        }

        public async Task GetAndCreateProductByEanAsync(string ean, CancellationToken none)
        {
            throw new NotImplementedException();
        }
    }
}
