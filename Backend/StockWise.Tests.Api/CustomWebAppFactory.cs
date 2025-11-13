using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StockWise.Application.Interfaces;
using StockWise.Infrastructure.Persistence;
using StockWise.Models;
using StockWise.Tests.Api.Fakes;
using System.IO;
using Microsoft.AspNetCore.Identity;

public class CustomWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.UseEnvironment("Test");
        builder.UseDefaultServiceProvider(options =>
        {
            options.ValidateScopes = true;
            options.ValidateOnBuild = true;
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<StockWiseDb>));
            services.RemoveAll(typeof(IEmailSenderService));
            services.AddSingleton<IEmailSenderService, FakeEmailSender>();


            var connString = GetConnectionString();
            services.AddSqlServer<StockWiseDb>(connString);

            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = FakeAuthHandler.Scheme;
                o.DefaultChallengeScheme = FakeAuthHandler.Scheme;
            })
            .AddScheme<AuthenticationSchemeOptions, FakeAuthHandler>(FakeAuthHandler.Scheme, _ => { });

            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            Seed(sp);
        });
    }

    private static string? GetConnectionString()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: true)
            .Build();

        return config.GetConnectionString("DefaultConnection");
    }

    public static void Seed(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        if (db.Companies.Any())
            return;

        var seller = AddCompany(db, "ACME", "1234567890", "123 Test Street", "acme@test.com", "123456789");
        var buyer = AddCompany(db, "Buyer Co", "9876543210", "456 Buyer Street", "buyer@test.com", "987654321");

        AddUsers(db, userManager, seller);

        var category = AddCategory(db, "Category");
        var product = AddProductWithInbound(db, seller, category,
                        name: "Test Product", ean: "12345678",
                        description: "Sample product for testing",
                        unitPricePln: 12.99m, inboundQty: 200);

        AddOrder(db, seller, buyer, product, orderedByUserName: "loginuser", quantity: 3);
    }

    private static Company AddCompany(StockWiseDb db, string name, string nip, string address, string email, string phone)
    {
        var c = new Company { Name = name, NIP = nip, Address = address, Email = email, Phone = phone };
        db.Companies.Add(c);
        db.SaveChanges();
        return c;
    }

    private static void AddUsers(StockWiseDb db, UserManager<AppUser> um, Company company)
    {
        db.Users.Add(new AppUser
        {
            Id = "u1",
            UserName = "john",
            Email = "john@test.com",
            EmailConfirmed = true,
            Company = company,
            CompanyMembershipStatus = CompanyMembershipStatus.Approved
        });

        var loginUser = new AppUser
        {
            UserName = "loginuser",
            Email = "loginuser@test.com",
            EmailConfirmed = true,
            CompanyId = company.Id,
            CompanyMembershipStatus = CompanyMembershipStatus.Approved
        };
        var create = um.CreateAsync(loginUser, "Password123!").GetAwaiter().GetResult();
        if (create.Succeeded)
        {
            um.AddToRoleAsync(loginUser, "Worker").GetAwaiter().GetResult();
        }

        var unconfirmedUser = new AppUser
        {
            UserName = "unconfirmeduser",
            Email = "nonexistent@test.com",
            EmailConfirmed = false,
            CompanyId = company.Id,
            CompanyMembershipStatus = CompanyMembershipStatus.Approved
        };
        var create2 = um.CreateAsync(unconfirmedUser, "Password123!").GetAwaiter().GetResult();
        if (create2.Succeeded) 
        {
            um.AddToRoleAsync(unconfirmedUser, "Worker").GetAwaiter().GetResult();
        }

        db.SaveChanges();
    }

    private static Category AddCategory(StockWiseDb db, string name)
    {
        var cat = new Category { Name = name };
        db.Categories.Add(cat);
        db.SaveChanges();
        return cat;
    }

    private static CompanyProduct AddProductWithInbound(
        StockWiseDb db,
        Company seller,
        Category category,
        string name,
        string ean,
        string description,
        decimal unitPricePln,
        int inboundQty)
    {
        var product = new CompanyProduct
        {
            CompanyProductName = name,
            EAN = ean,
            Description = description,
            Price = Money.Of(unitPricePln, "PLN"),
            Stock = inboundQty,
            Company = seller,
            IsAvailableForOrder = true,
            Category = category,
            InventoryMovements = new List<InventoryMovement>()
        };

        var movement = new InventoryMovement
        {
            Date = DateTime.UtcNow,
            Type = MovementType.Inbound,
            Quantity = inboundQty,
            Comment = "Initial stock",
            CompanyProduct = product
        };

        product.InventoryMovements.Add(movement);

        db.CompanyProducts.Add(product);
        db.SaveChanges();
        return product;
    }

    private static void AddOrder(
        StockWiseDb db,
        Company seller,
        Company buyer,
        CompanyProduct product,
        string orderedByUserName,
        int quantity)
    {
        var order = new Order
        {
            Seller = seller,
            Buyer = buyer,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UserNameWhoMadeOrder = orderedByUserName
        };

        var orderItem = new OrderProduct
        {
            CompanyProductId = product.CompanyProductId,
            Quantity = quantity,
        };

        order.ProductsWithQuantity.Add(orderItem);

        order.TotalPrice = Money.Of(product.Price.Amount * quantity, product.Price.Currency.Code);
        db.Orders.Add(order);
        db.SaveChanges();
    }
}
