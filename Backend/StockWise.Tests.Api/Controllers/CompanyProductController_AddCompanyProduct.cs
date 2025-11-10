using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace StockWise.Tests.Api.Controllers
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
            var client = _factory.CreateClient();

            var form = new MultipartFormDataContent
            {
                { new StringContent("Logitech M185 Wireless Mouse"), "CompanyProductName" },
                { new StringContent("5099206027295"), "EAN" },
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
