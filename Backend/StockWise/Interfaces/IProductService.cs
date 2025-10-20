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
        Task<ServiceResult<Product>> AddProduct(CreateProductDto productDto);
        Task<ServiceResult<Product>> DeleteProduct(int id);
        Task<ServiceResult<Product>> UpdateProduct(UpdateProductDto productDto);
        string GetCategoryFullPath(Models.Category category);
    }
}
