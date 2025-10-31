using Microsoft.EntityFrameworkCore;
using StockWise.Application.Interfaces;
using StockWise.Infrastructure.Persistence;
using StockWise.Models;

namespace StockWise.Infrastructure.Services
{
    public class EanService : IEanService
    {
        private readonly StockWiseDb _context;
        public EanService(StockWiseDb context)
        {
            _context = context;
        }
        public async Task<Category> EnsureCategoryHierarchyAsync(string fullCategoryPath)
        {
            var categoryNames = fullCategoryPath.Split(">").Select(s => s.Trim()).ToList();
            Category? parent = null;

            foreach (var name in categoryNames)
            {
                int? parentId = parent?.CategoryId;

                var existing = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name == name && c.ParentId == parentId);

                if (existing == null)
                {
                    var newCategory = new Category
                    {
                        Name = name,
                        Parent = parent
                    };

                    _context.Categories.Add(newCategory);
                    await _context.SaveChangesAsync();
                    parent = newCategory;
                }
                else
                {
                    parent = existing;
                }
            }

            return parent!;
        }
    }
}
