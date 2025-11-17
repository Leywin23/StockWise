using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Net;
using Newtonsoft.Json;
using StockWise.Application.Contracts.ProductDtos;
using Microsoft.Extensions.DependencyInjection;
using StockWise.Infrastructure.Persistence;

namespace StockWise.Tests.Api.Controllers.ProductController_Tests
{
    public class ProductController_GetProductsTest : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        public ProductController_GetProductsTest(CustomWebAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetProducts_ShouldReturnOk_AndListOfProducts()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("api/Product");
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);

            var products = JsonConvert.DeserializeObject<List<ProductDto>>(body);

            products.Should().NotBeNull();
            products.Should().BeOfType<List<ProductDto>>();
            products.Should().NotBeEmpty();
            products!.First().ProductName.Should().NotBeNullOrEmpty();
        }
    }
}
