using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StockWise.Data;
using StockWise.Exceptions;
using StockWise.Filters;
using StockWise.Helpers;
using StockWise.Hubs;
using StockWise.Interfaces;
using StockWise.Models;
using StockWise.Services;
using System.Security.Claims;

namespace StockWise
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<StockWiseDb>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            // Add services to the container.
            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<ModelStateValidationFilter>();
            })
            .ConfigureApiBehaviorOptions(o =>
                {
                    o.SuppressModelStateInvalidFilter = true;
                })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.WriteIndented = true;
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });

            builder.Services.AddRazorPages();
            builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer"                   
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
            new string[] {}
        }});
        });

        builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;
        })
        .AddRoles<IdentityRole>()           
        .AddEntityFrameworkStores<StockWiseDb>();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["JWT:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWT:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"])),

                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role
                };
            });


            builder.Services.AddHttpClient();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
                });
            });

            builder.Services.AddMemoryCache();
            builder.Services.AddSignalR();
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<ICompanyService, CompanyService>();
            builder.Services.AddScoped<ICompanyProductService, CompanyProductService>();
            builder.Services.AddScoped<IEanService, EanService>();
            builder.Services.AddScoped<IInventoryMovementService, InventoryMovementService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddTransient<IEmailSenderServicer, EmailSenderService>();
            builder.Services.AddSingleton<ApiExchangeRateProvider>();
            builder.Services.AddSingleton<IExchangeRateProvider>(sp =>
                {
                    var inner = sp.GetRequiredService<ApiExchangeRateProvider>();
                    var cache = sp.GetRequiredService<IMemoryCache>();
                    var logger = sp.GetRequiredService<ILogger<CachedExchangeRateProvider>>();
                    var ttl = TimeSpan.FromMinutes(10);

                    return new CachedExchangeRateProvider(inner, cache, logger, ttl);
                }
            );

            builder.Services.AddSingleton<MoneyConverter>();

            var app = builder.Build();

            app.UseMiddleware<GlobalExceptionMiddleware>();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors("AllowFrontend");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapHub<StockHub>("/stockHub");
            app.MapControllers();
            app.MapRazorPages();


            app.Run();
            
         }
    }
}
