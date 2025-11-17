using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StockWise.Application.Contracts.ProductDtos;
using StockWise.Infrastructure.Persistence;
using StockWise.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StockWise.Tests.Api.Controllers.ProductController_Tests
{
    public class ProductController_GetProductByIdTest : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        public ProductController_GetProductByIdTest(CustomWebAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetProductById_ShouldReturnOkAndProduct()
        {
            var productId = 0;
            Product expected;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();

                expected = db.Products.First();
                var product = db.Products.First();

                productId = product.ProductId;
            }

            var client = _factory.CreateClient();
            var resp = await client.GetAsync($"api/Product/{productId}");
            var body = await resp.Content.ReadAsStringAsync();
            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);

            var returned = await resp.Content.ReadFromJsonAsync<ProductDto>(
             new System.Text.Json.JsonSerializerOptions
             {
                 PropertyNameCaseInsensitive = true
             });

            returned.Should().NotBeNull();
            returned.ProductName.Should().Be(expected.ProductName);
            returned.Ean.Should().Be(expected.EAN);
        }

        [Fact]
        public async Task GetProductId_ShouldReturnNotFound()
        {
            int productId = 999;
            var clent = _factory.CreateClient();
            var resp = await clent.GetAsync($"api/Product/{productId}");
            var body =  await resp.Content.ReadAsStringAsync();
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound, body);
            body.Should().NotBeNull();
            body.Should().NotBeEmpty();
            body.Should().Contain($"Product with id: {productId} not found");
        }
    }
}
