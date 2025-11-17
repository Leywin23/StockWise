using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using StockWise.Infrastructure.Persistence;
using StockWise.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StockWise.Tests.Api.Controllers.ProductController_Tests
{
    public class ProductController_DeleteProductTest : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;

        public ProductController_DeleteProductTest(CustomWebAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task DeleteProduct_ShouldReturnOk_AndRemoveProductFromDatabase()
        {
            int productId;
            string ean;

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
                var product = db.Products.First();     
                productId = product.ProductId;
                ean = product.EAN;
            }

            var client = _factory.CreateClient();
            var resp = await client.DeleteAsync($"api/Product/{productId}");
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);
            body.Should().NotBeNullOrEmpty();

            var deleted = JsonConvert.DeserializeObject<Product>(body);
            deleted.Should().NotBeNull();
            deleted!.ProductId.Should().Be(productId);
            deleted.EAN.Should().Be(ean);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
                var productInDb = db.Products.FirstOrDefault(p => p.ProductId == productId);
                productInDb.Should().BeNull();
            }
        }

        [Fact]
        public async Task DeleteProduct_ShouldReturnNotFound_WhenProductDoesNotExist()
        {
            int notExistingId;

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
                var maxId = db.Products.Any() ? db.Products.Max(p => p.ProductId) : 0;
                notExistingId = maxId + 1000;
            }

            var client = _factory.CreateClient();
            var resp = await client.DeleteAsync($"api/Product/{notExistingId}");
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
            body.Should().NotBeNullOrEmpty();
            body.Should().Contain($"Couldn't find a product with given id: {notExistingId}");
        }

        [Fact]
        public async Task DeleteProduct_ShouldReturnOk_WhenProductHasImage()
        {
            int productId;

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
                var productWithImage = db.Products.FirstOrDefault(p => p.Image != null);

                if (productWithImage == null)
                {
                    var p = db.Products.First();
                    p.Image = "https://test/image.png";
                    db.SaveChanges();
                    productId = p.ProductId;
                }
                else
                {
                    productId = productWithImage.ProductId;
                }
            }

            var client = _factory.CreateClient();
            var resp = await client.DeleteAsync($"api/Product/{productId}");
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
                var productInDb = db.Products.FirstOrDefault(p => p.ProductId == productId);
                productInDb.Should().BeNull();
            }
        }
    }
}
