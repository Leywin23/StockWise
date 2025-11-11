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

            if (!db.Companies.Any())
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
                db.SaveChanges();

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
                var create = userManager.CreateAsync(loginUser, "Password123!").GetAwaiter().GetResult();
                if (create.Succeeded)
                {
                    userManager.AddToRoleAsync(loginUser, "Worker").GetAwaiter().GetResult();
                }
                var unconfirmedUser = new AppUser
                {
                    UserName = "unconfirmeduser",
                    Email = "nonexistent@test.com",
                    EmailConfirmed = false, 
                    CompanyId = company.Id,
                    CompanyMembershipStatus = CompanyMembershipStatus.Approved
                };

                var create2 = userManager.CreateAsync(unconfirmedUser, "Password123!").GetAwaiter().GetResult();
                if (create.Succeeded)
                {
                    userManager.AddToRoleAsync(unconfirmedUser, "Worker").GetAwaiter().GetResult();
                }

                var category = new Category { Name = "Category" };
                db.Categories.Add(category);

                var companyProduct = new CompanyProduct
                {
                    CompanyProductName = "Test Product",
                    EAN = "12345678",
                    Description = "Sample product for testing",
                    Price = Money.Of(12.99m, "PLN"),
                    Stock = 200,
                    Company = company,
                    IsAvailableForOrder = true,
                    Category = category,
                    InventoryMovements = new List<InventoryMovement>()
                };

                var inventoryMovement = new InventoryMovement
                {
                    Date = DateTime.Now,
                    Type = MovementType.Inbound,
                    Quantity = 200,
                    Comment = "Initial stock",
                    CompanyProduct = companyProduct 
                };

                companyProduct.InventoryMovements.Add(inventoryMovement);

                db.CompanyProducts.Add(companyProduct);

                db.SaveChanges();
            }
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

}
