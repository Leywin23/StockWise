using StockWise.Models;

namespace StockWise.Interfaces
{
    public interface IEanService
    {
        Task<Category> EnsureCategoryHierarchyAsync(string fullCategoryPath);
    }
}
