using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StockWise.Tests.Api.Controllers.CompanyProductController_Tests
{
    public class CompanyProductController_PutCompanyProduct : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        public CompanyProductController_PutCompanyProduct(CustomWebAppFactory factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData(1)]
        public async Task UpdateCompanyProductAsync_ShouldReturnOK(int id)
        {
            var client = _factory.CreateClient();
            var form = new MultipartFormDataContent
            {
                { new StringContent("Keychron K6 Wireless Mechanical Keyboard"), "CompanyProductName" },
                { new StringContent("Compact 65% wireless mechanical keyboard with RGB backlight and hot-swappable switches."), "Description" },
                { new StringContent("Keyboards"), "CategoryName" },
                { new StringContent("89,99"), "Price" },
                { new StringContent("USD"), "Currency" },
                { new StringContent("85"), "Stock" },
                { new StringContent("true"), "IsAvailableForOrder" }
            };
            var resp = await client.PutAsync($"api/CompanyProduct/{id}", form);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);
        }
        [Theory]
        [InlineData(2)]
        public async Task UpdateCompanyProductAsync_ShouldReturnNotFound(int id)
        {
            var client = _factory.CreateClient();
            var form = new MultipartFormDataContent
            {
                { new StringContent("Keychron K6 Wireless Mechanical Keyboard"), "CompanyProductName" },
                { new StringContent("Compact 65% wireless mechanical keyboard with RGB backlight and hot-swappable switches."), "Description" },
                { new StringContent("Keyboards"), "CategoryName" },
                { new StringContent("89,99"), "Price" },
                { new StringContent("USD"), "Currency" },
                { new StringContent("85"), "Stock" },
                { new StringContent("true"), "IsAvailableForOrder" }
            };
            var resp = await client.PutAsync($"api/CompanyProduct/{id}", form);
            var body = resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
