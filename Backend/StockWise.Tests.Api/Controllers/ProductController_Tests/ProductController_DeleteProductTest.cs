using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
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
            string ean = RandomEan8();

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();

                var cat = db.Categories.FirstOrDefault();
                if (cat == null)
                {
                    cat = new Category { Name = "DelProdCat" };
                    db.Categories.Add(cat);
                    db.SaveChanges();
                }

                var product = new Product
                {
                    ProductName = "ToDelete",
                    EAN = ean,
                    Description = "to-delete",
                    ShoppingPrice = Money.Of(10m, "PLN"),
                    SellingPrice = Money.Of(12m, "PLN"),
                    CategoryId = cat.CategoryId
                };

                db.Products.Add(product);
                db.SaveChanges();
                productId = product.ProductId;
            }

            var client = _factory.CreateClient();
            var resp = await client.DeleteAsync($"/api/Product/{productId}");
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);

            body.Should().NotBeNullOrWhiteSpace();
            body.Should().Contain(ean);        
            body.Should().Contain("ToDelete"); 

            using (var scope2 = _factory.Services.CreateScope())
            {
                var db2 = scope2.ServiceProvider.GetRequiredService<StockWiseDb>();
                db2.Products.FirstOrDefault(p => p.ProductId == productId).Should().BeNull();
            }
        }

        private static string RandomEan8()
        {
            var digits = new string(Guid.NewGuid().ToString("N").Where(char.IsDigit).ToArray());
            if (digits.Length < 7) digits = digits.PadRight(7, '1');
            return "9" + digits.Substring(0, 7);
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
