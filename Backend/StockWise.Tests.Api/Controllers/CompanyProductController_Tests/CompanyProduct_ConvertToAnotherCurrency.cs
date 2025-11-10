using FluentAssertions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace StockWise.Tests.Api.Controllers.CompanyProductController_Tests
{
    public class CompanyProduct_ConvertToAnotherCurrency : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        public CompanyProduct_ConvertToAnotherCurrency(CustomWebAppFactory factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData(1, "USD")]
        public async Task ConvertToAnotherCurrency_ShouldReturnOk(int id, string toCode)
        {
            var client = _factory.CreateClient();

            var resp = await client.GetAsync($"api/CompanyProduct/{id}/convert?toCode={toCode}");
            var body = await resp.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            root.TryGetProperty("currency", out var currencyElement).Should().BeTrue(body);
            root.TryGetProperty("amount", out var amountElement).Should().BeTrue(body);

            currencyElement.TryGetProperty("code", out var codeProp).Should().BeTrue(body);

            codeProp.GetString().Should().Be($"{toCode}");
            amountElement.GetDecimal().Should().BeGreaterThan(0);
            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);
        }
        [Fact]
        public async Task Convert_ShouldRetrunBadRequest_WhenToCodeMissing()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("api/CompanyProduct/1/convert");
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
        }

        [Fact]
        public async Task ShouldReturn_NotFound_WhenProductIsMissing()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("api/CompanyProduct/999/convert?toCode=USD");
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.NotFound, body);
        }

    }
}
