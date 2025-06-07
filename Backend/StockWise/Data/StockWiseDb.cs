using Microsoft.EntityFrameworkCore;
using StockWise.Models;

namespace StockWise.Data
{
    public class StockWiseDb: DbContext
    {
        public StockWiseDb(DbContextOptions<StockWiseDb> options): base(options){}

        public DbSet<Product> products { get; set; }
        public DbSet<Category> categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>()
                .HasOne(c => c.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>().
                HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}
