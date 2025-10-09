using System.ComponentModel.DataAnnotations;

namespace StockWise.Models
{
    public class Category
    {
        [Key]
        public int CategoryId {  get; set; }
        [Required]
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public Category? Parent { get; set; }
        public List<Category>? Children { get; set; }
        public List<Product>? Products { get; set; }
        public List<CompanyProduct>? CompanyProducts { get; set; }
    }
}
