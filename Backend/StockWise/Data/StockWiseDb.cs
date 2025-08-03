using Microsoft.EntityFrameworkCore;
using StockWise.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace StockWise.Data
{
    public class StockWiseDb: IdentityDbContext<AppUser>
    {
        public StockWiseDb(DbContextOptions<StockWiseDb> options): base(options){}

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<InventoryMovement> InventoryMovement { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyProduct> CompanyProducts { get; set; }

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
                HasOne(im => im.CompanyProduct)
                .WithMany(p => p.InventoryMovements)
                .HasForeignKey(im => im.CompanyProductId);

            modelBuilder.Entity<Order>()
                .HasOne(o=>o.Buyer)
                .WithMany(c=>c.OrdersAsBuyer)
                .HasForeignKey(o=>o.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o=>o.Seller)
                .WithMany(c=>c.OrdersAsSeller)
                .HasForeignKey(o=>o.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AppUser>()
                .HasOne(u => u.Company)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CompanyProduct>()
                .HasOne(cp=>cp.Company)
                .WithMany(c=>c.CompanyProducts)
                .HasForeignKey(cp=>cp.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);


            SeedRoles(modelBuilder);
        }

        private void SeedRoles(ModelBuilder builder)
        {
            List<IdentityRole> roles = new List<IdentityRole>()
            {
                new IdentityRole
                {
                    Id = "1",
                    Name = "Manager",
                    NormalizedName = "MANAGER",
                },
                new IdentityRole
                {
                    Id = "2",
                    Name = "Worker",
                    NormalizedName = "Worker",
                },
            };
            builder.Entity<IdentityRole>().HasData(roles);
            }
        }
}
