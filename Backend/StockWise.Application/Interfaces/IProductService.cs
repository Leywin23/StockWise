using Microsoft.AspNetCore.Mvc;
using StockWise.Application.Abstractions;
using StockWise.Application.Contracts.ProductDtos;
using StockWise.Models;

namespace StockWise.Application.Interfaces
{
    public interface IProductService
    {
        Task<ServiceResult<ProductDto>> GetProductById(int id);
        Task<ServiceResult<List<ProductDto>>> GetProducts();
        Task<ServiceResult<Product>> AddProduct(CreateProductDto productDto, CancellationToken ct = default);
        Task<ServiceResult<Product>> DeleteProduct(int id, CancellationToken ct = default);
        Task<ServiceResult<Product>> UpdateProduct(int productId, UpdateProductDto productDto, CancellationToken ct = default);
        string GetCategoryFullPath(Models.Category category);
    }
}
