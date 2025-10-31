using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StockWise.Application.Interfaces;
using StockWise.Domain;
using StockWise.Infrastructure.HostedService;
using StockWise.Infrastructure.Persistence;
using StockWise.Infrastructure.Services;
using StockWise.Models;
using System;
using System.Security.Claims;

namespace StockWise.Infrastructure.Configuration
{
    public static class InfrastructureInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration cfg)
        {
            services.AddDbContext<StockWiseDb>(options =>
                options.UseSqlServer(cfg.GetConnectionString("DefaultConnection")));

            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<StockWiseDb>()
            .AddDefaultTokenProviders();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = cfg["JWT:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = cfg["JWT:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(cfg["JWT:SigningKey"])),
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role
                };
            });

            services.AddHttpClient();
            services.AddMemoryCache();

            services.Configure<AzureStorageOptions>(opt =>
            {
                opt.ConnectionString = cfg["AzureStorage:ConnectionString"] ?? "";
                opt.ContainerName = cfg["AzureStorage:ContainerName"] ?? "stockwiseimages";
            });
            services.AddSingleton(sp =>
            {
                var opts = sp.GetRequiredService<IOptions<AzureStorageOptions>>().Value;
                return new BlobServiceClient(opts.ConnectionString);
            });

            services.Configure<EmailSettings>(cfg.GetSection("EmailSettings"));
            services.AddTransient<IEmailSenderService, EmailSenderService>();

            services.AddScoped<BlobStorageService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<ICompanyProductService, CompanyProductService>();
            services.AddScoped<IEanService, EanService>();
            services.AddScoped<IInventoryMovementService, InventoryMovementService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IAccountService, AccountService>();

            services.AddSingleton<ApiExchangeRateProvider>();
            services.AddSingleton<IExchangeRateProvider>(sp =>
            {
                var inner = sp.GetRequiredService<ApiExchangeRateProvider>();
                var cache = sp.GetRequiredService<IMemoryCache>();
                var logger = sp.GetRequiredService<ILoggerFactory>()
                               .CreateLogger<CachedExchangeRateProvider>();
                var ttl = TimeSpan.FromMinutes(10);

                return new CachedExchangeRateProvider(inner, cache, logger, ttl);
            });

            services.AddHostedService<RevokedTokensCleanup>();
            services.AddHostedService<UnverifiedCompanyCleanup>();

            services.AddSingleton<MoneyConverter>();

            return services;
        }
    }
}
