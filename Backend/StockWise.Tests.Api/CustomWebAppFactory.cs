using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockWise;
using StockWise.Infrastructure.Persistence;
using StockWise.Models;
using StockWise.Tests.Api;
using System.Linq;

public class CustomWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((ctx, cfg) =>
        {
            cfg.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
               .AddJsonFile("appsettings.Test.json", optional: true, reloadOnChange: false);
        });

        builder.ConfigureServices(services =>
        {
            // usuń oryginalną rejestrację DbContextu
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<StockWiseDb>));
            if (descriptor != null)
                services.Remove(descriptor);

            // dodaj testowy DbContext (SQL Server LocalDB -> StockWise_Test)
            services.AddDbContext<StockWiseDb>((sp, options) =>
            {
                var cfg = sp.GetRequiredService<IConfiguration>();
                var cs = cfg.GetConnectionString("DefaultConnection")!;
                options.UseSqlServer(cs);
            });

            // podmień autoryzację na fake
            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = FakeAuthHandler.Scheme;
                o.DefaultChallengeScheme = FakeAuthHandler.Scheme;
            }).AddScheme<AuthenticationSchemeOptions, FakeAuthHandler>(FakeAuthHandler.Scheme, _ => { });

            // seed danych
            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
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
                db.Users.Add(new AppUser
                {
                    Id = "u1",
                    UserName = "john",
                    Email = "john@test.com",
                    Company = company,
                    CompanyMembershipStatus = CompanyMembershipStatus.Approved
                });

                db.SaveChanges();
            }
        });
    }
}
