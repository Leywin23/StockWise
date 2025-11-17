using StockWise.Application.Abstractions;
using StockWise.Models;

namespace StockWise.Application.Interfaces
{
    public interface IEanService
    {
        Task<ServiceResult<Product>> GetAndCreateProductByEanAsync(string ean, CancellationToken ct = default);
        Task<Category> EnsureCategoryHierarchyAsync(string fullCategoryPath);
    }
}
