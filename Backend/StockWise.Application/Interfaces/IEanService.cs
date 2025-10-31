using StockWise.Models;

namespace StockWise.Application.Interfaces
{
    public interface IEanService
    {
        Task<Category> EnsureCategoryHierarchyAsync(string fullCategoryPath);
    }
}
