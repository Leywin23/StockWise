using Microsoft.AspNetCore.Mvc;
using StockWise.Dtos.ProductDtos;
using StockWise.Models;
using StockWise.Response;

namespace StockWise.Interfaces
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
