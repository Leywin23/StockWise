using StockWise.Data;
using Microsoft.EntityFrameworkCore;
using StockWise.Hubs;
using StockWise.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StockWise.Interfaces;
using StockWise.Services;

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
                builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = true;
                });
                builder.Services.AddRazorPages();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(options =>
                    {
                        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                        {
                            Name = "Authorization",
                            In = ParameterLocation.Header,
                            Type = SecuritySchemeType.Http,
                            Scheme = "Bearer"
                        });
                    }
                );

            builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
            }).AddEntityFrameworkStores<StockWiseDb>();

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
                            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"]))
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

                builder.Services.AddSignalR();
                builder.Services.AddScoped<ITokenService, TokenService>();

                var app = builder.Build();
                app.MapHub<StockHub>("/stockHub");
                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }
                app.MapControllers();
                app.UseHttpsRedirection();
                app.UseStaticFiles();

                app.UseCors("AllowFrontend");

                app.UseRouting();

                app.UseAuthentication();
                app.UseAuthorization();

                app.MapRazorPages();

                app.Run();
            
         }
    }
}
