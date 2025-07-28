using Microsoft.EntityFrameworkCore;
using StockWise.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace StockWise.Data
{
    public class StockWiseDb: IdentityDbContext 
    {
        public StockWiseDb(DbContextOptions<StockWiseDb> options): base(options){}

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<InventoryMovement> InventoryMovement { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Company> Companies { get; set; }

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

            modelBuilder.Entity<InventoryMovement>().
                HasOne(im => im.Product)
                .WithMany(p => p.InventoryMovements)
                .HasForeignKey(im => im.ProductId);

            modelBuilder.Entity<Order>()
                .HasOne(o=>o.Buyer)
                .WithMany(c=>c.OrdersAsBuyer)
                .HasForeignKey(o=>o.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o=>o.Seller)
                .WithMany(c=>c.OrdersAsSeller)
                .HasForeignKey(o=>o.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AppUser>()
                .HasOne(u => u.Company)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
