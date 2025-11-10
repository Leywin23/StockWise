using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using StockWise.Infrastructure.Persistence;
using StockWise.Models;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using Xunit;
using Xunit.Abstractions;

namespace StockWise.Tests.Api.Controllers.CompanyProductController_Tests
{
    public class CompanyProductController_AddCompanyProduct : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        private readonly ITestOutputHelper _output;

        public CompanyProductController_AddCompanyProduct(CustomWebAppFactory factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;
        }

        [Fact]
        public async Task AddCompanyProduct_ShouldReturnOk()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
                await db.Database.EnsureDeletedAsync();
                await db.Database.EnsureCreatedAsync();

                var company = new Company { Name = "ACME", NIP = "1234567890", Address = "A 1", Email = "a@a", Phone = "123" };
                db.Companies.Add(company);
                db.Users.Add(new AppUser { Id = "u1", UserName = "john", Email = "john@test.com", EmailConfirmed = true, Company = company, CompanyMembershipStatus = CompanyMembershipStatus.Approved });
                db.Categories.Add(new Category { Name = "Category" });
                await db.SaveChangesAsync();
            }
            var client = _factory.CreateClient();

            var form = new MultipartFormDataContent
            {
                { new StringContent("Logitech M185 Wireless Mouse"), "CompanyProductName" },
                { new StringContent("5099206027291"), "EAN" },
                { new StringContent("Compact wireless optical mouse Logitech M185 ..."), "Description" },
                { new StringContent("Category"), "Category" },
                { new StringContent("12,99"), "Price" },
                { new StringContent("USD"), "Currency" },
                { new StringContent("150"), "Stock" },
                { new StringContent("true"), "IsAvailableForOrder" }
            };

            var resp = await client.PostAsync("api/CompanyProduct", form);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.OK, $"response body was: {body}");
        }
        [Fact]
        public async Task AddCompanyProduct_ShouldReturnBadRequest()
        {
            var client = _factory.CreateClient();
            var form = new MultipartFormDataContent
            {
                { new StringContent("Keychron K6 Wireless Mechanical Keyboard"), "CompanyProductName" },
                { new StringContent("1234567"), "EAN" },
                { new StringContent("Compact 65% wireless mechanical keyboard with RGB backlight and hot-swappable switches."), "Description" },
                { new StringContent("Keyboards"), "Category" },
                { new StringContent("89,99"), "Price" },
                { new StringContent("USD"), "Currency" },
                { new StringContent("85"), "Stock" },
                { new StringContent("true"), "IsAvailableForOrder" }
            };
            var resp = await client.PostAsync("api/CompanyProduct", form);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
        }
        [Fact]
        public async Task AddCompanyProduct_ShouldReturnBadRequest_ProductAlreadyExist()
        {
            var client = _factory.CreateClient();
            var form = new MultipartFormDataContent
            {
                { new StringContent("Logitech M185 Wireless Mouse"), "CompanyProductName" },
                { new StringContent("5099206027295"), "EAN" },
                { new StringContent("Compact 65% wireless mechanical keyboard with RGB backlight and hot-swappable switches."), "Description" },
                { new StringContent("Keyboards"), "Category" },
                { new StringContent("89,99"), "Price" },
                { new StringContent("USD"), "Currency" },
                { new StringContent("85"), "Stock" },
                { new StringContent("true"), "IsAvailableForOrder" }
            };
            var resp = await client.PostAsync("api/CompanyProduct", form);
            var body = await resp.Content.ReadAsStringAsync();
        }
    }
}
