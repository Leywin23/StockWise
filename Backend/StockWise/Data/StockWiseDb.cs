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

            modelBuilder.Entity<OrderProduct>()
     .HasKey(op => new { op.OrderId, op.CompanyProductId });

            modelBuilder.Entity<Order>()
                .HasMany(o => o.ProductsWithQuantity)
                .WithOne(op => op.OrderTable)
                .HasForeignKey(op => op.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderProduct>()
                .HasOne(op => op.CompanyProduct)
                .WithMany()                                   
                .HasForeignKey(op => op.CompanyProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderProduct>()
                .Property(op => op.Quantity)
                .IsRequired();

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

            modelBuilder.Entity<Product>(b =>
            {
                b.OwnsOne(p => p.ShoppingPrice, money =>
                {
                    money.Property(m => m.Amount)
                         .HasColumnType("decimal(18,2)")
                         .HasColumnName("ShoppingPriceAmount");

                    money.OwnsOne(m => m.Currency, curr =>
                    {
                        curr.Property(c => c.Code)
                            .HasMaxLength(3)
                            .HasColumnName("ShoppingPriceCurrency");
                    });
                });

                b.OwnsOne(p => p.SellingPrice, money =>
                {
                    money.Property(m => m.Amount)
                         .HasColumnType("decimal(18,2)")
                         .HasColumnName("SellingPriceAmount");

                    money.OwnsOne(m => m.Currency, curr =>
                    {
                        curr.Property(c => c.Code)
                            .HasMaxLength(3)
                            .HasColumnName("SellingPriceCurrency");
                    });
                });
            });

            modelBuilder.Entity<CompanyProduct>(b =>
            {
                
                b.OwnsOne(p => p.Price, money =>
                {
                    money.Property(m => m.Amount)
                         .HasColumnType("decimal(18,2)")
                         .HasColumnName("CompanyPriceAmount");

                    money.OwnsOne(m => m.Currency, curr =>
                    {
                        curr.Property(c => c.Code)
                            .HasMaxLength(3)
                            .HasColumnName("CompanyPriceCurrency");
                    });
                });
            });

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
                    NormalizedName = "WORKER",
                },
            };
            builder.Entity<IdentityRole>().HasData(roles);
            }
        }
}
