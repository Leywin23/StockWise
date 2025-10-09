using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Dtos.ProductDtos;
using StockWise.Helpers;
using StockWise.Interfaces;
using StockWise.Mappers;
using StockWise.Models;
namespace StockWise.Services
{
    public class ProductService: IProductService
    {
        private readonly StockWiseDb _context;
        private readonly IMapper _mapper;
        public ProductService(StockWiseDb context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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

        public async Task<ServiceResult<Product>> AddProduct(CreateProductDto productDto)
        {
            if (productDto == null)
            {
                return ServiceResult<Product>.BadRequest("Product data is required.");
            }

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == productDto.Category);

            if (category == null)
            {
                category = new Models.Category { Name = productDto.Category };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
            }

            var product = productDto.ToProductFromCreate(category);
            var exist = await _context.Products.FirstOrDefaultAsync(p=>p.EAN == productDto.EAN);
            if(exist != null)
            {
                return ServiceResult<Product>.BadRequest($"Product with EAN {exist.EAN} already added");
            }

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return ServiceResult<Product>.Ok(product);
        }

        public async Task<ServiceResult<Product>> DeleteProduct(int id)
        {
            var productToDelete = await _context.Products.FindAsync(id);
            if (productToDelete == null)
            {
                return ServiceResult<Product>.NotFound($"Couldn't find a product with given id: {id}");
            }
            _context.Products.Remove(productToDelete);
            await _context.SaveChangesAsync();
            var productToDeleteDto = _mapper.Map<Models.Product>(productToDelete);
            return ServiceResult<Product>.Ok(productToDelete);
        }

        public async Task<ServiceResult<Product>> UpdateProduct(UpdateProductDto productDto)
        {
            var productToUpdate = await _context.Products.FirstOrDefaultAsync(x => x.EAN == productDto.EAN);
            if (productToUpdate == null)
                return ServiceResult<Product>.NotFound("Couldn't find a product");

            var category = await _context.Categories.FirstOrDefaultAsync(x => x.Name == productDto.CategoryName);
            if (category == null)
            {
                return ServiceResult<Product>.NotFound("Coundn't find a category with this name");
            }
            var shoppingPrice = Money.Of(productDto.ShoppingPrice, productDto.Currency.Code);
            var sellingPrice = Money.Of(productDto.SellingPrice, productDto.Currency.Code);

            productToUpdate.ProductName = productDto.ProductName;
            productToUpdate.CategoryId = category.CategoryId;
            productToUpdate.Description = productDto.Description;
            productToUpdate.ShoppingPrice = shoppingPrice;
            productToUpdate.SellingPrice = sellingPrice;
            productToUpdate.Image = productDto.Image;

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

    }
}
