using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockWise;
using StockWise.Infrastructure.Persistence;
using StockWise.Models;
using StockWise.Tests.Api;
using System.Data.Common;
using System.Linq;

public class CustomWebAppFactory : WebApplicationFactory<Program>
{
    private static readonly object _seedLock = new();
    private static bool _seedDone;
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        builder.ConfigureAppConfiguration((ctx, ctg) =>
        {
            ctg.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
            ctg.AddJsonFile("appsettings.Test.json", optional: true, reloadOnChange: false);
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<StockWiseDb>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<StockWiseDb>((sp, options) =>
            {
                var cfg = sp.GetRequiredService<IConfiguration>();
                var cs = cfg.GetConnectionString("DefaultConnection")!;
                options.UseSqlServer(cs);
            });
            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = FakeAuthHandler.Scheme;
                o.DefaultChallengeScheme = FakeAuthHandler.Scheme;
            })
            .AddScheme<AuthenticationSchemeOptions, FakeAuthHandler>(FakeAuthHandler.Scheme, _ => { });
            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
            db.Database.EnsureCreated();
            if (!_seedDone)
            {
                lock (_seedLock)
                {
                    if (!_seedDone)
                    {
                        Seed(db);
                        _seedDone = true;
                    }
                }
            }
        });
    }
    private static void Seed(StockWiseDb db)
    {
        var company = new Company
        {
            Name = "ACME",
            NIP = "1234567890",
            Address = "123 Test Street",
            Email = "acme@test.com",
            Phone = "123456789"
        };

        db.Companies.Add(company);

        db.Users.Add(new AppUser
        {
            Id = "u1",
            UserName = "john",
            Email = "john@test.com",
            Company = company,
            CompanyMembershipStatus = CompanyMembershipStatus.Approved
        });

        var category = new Category
        {
            Name = "Category"
        };

        db.Categories.Add(category);

        db.CompanyProducts.Add(new CompanyProduct
        {
            CompanyProductName = "Test Product",
            EAN = "12345678",
            Description = "Sample product for testing",
            Price = Money.Of(12.99m, "PLN"),
            Stock = 200,
            Company = company,
            IsAvailableForOrder = true,
            Category = category,
        });

        db.SaveChanges();
    }
}