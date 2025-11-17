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
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StockWise.Tests.Api.Controllers.ProductController_Tests
{
    public class ProductController_UpdateProductTest : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        public ProductController_UpdateProductTest(CustomWebAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task UpdateProductTest_ShouldReturnOkAndUpadateProduct()
        {

            int productId = 0;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
                var product = db.Products.First();
                productId = product.ProductId;
            }
            var form = new MultipartFormDataContent
            {
                { new StringContent("Test1"), "ProductName" },
                { new StringContent("Description"), "Description" },
                { new StringContent("12"), "ShoppingPrice" },
                { new StringContent("14"), "SellingPrice" },
                { new StringContent("PLN"), "Currency" },
                { new StringContent("Category"), "CategoryName" }
            };

            var client = _factory.CreateClient();
            var resp = await client.PutAsync($"api/Product/{productId}", form);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);

            body.Should().NotBeNullOrEmpty();

            var result = JsonConvert.DeserializeObject<Product>(body);
            result.Should().NotBeNull();
            result!.Should().BeOfType<Product>();

            result.ProductName.Should().Be("Test1");
            result.Description.Should().Be("Description");

            result.ShoppingPrice.Amount.Should().Be(12);
            result.ShoppingPrice.Currency.Code.Should().Be("PLN");

            result.SellingPrice.Amount.Should().Be(14);
            result.SellingPrice.Currency.Code.Should().Be("PLN");


            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
                var updated = db.Products
                    .Include(p => p.Category)
                    .First(p => p.ProductId == productId);

                updated.ProductName.Should().Be("Test1");
                updated.Description.Should().Be("Description");

                updated.ShoppingPrice.Amount.Should().Be(12);
                updated.ShoppingPrice.Currency.Code.Should().Be("PLN");

                updated.SellingPrice.Amount.Should().Be(14);
                updated.SellingPrice.Currency.Code.Should().Be("PLN");
            }
        }
        [Fact]
        public async Task UpdateProductTest_ShouldReturnIdNotFound()
        {
            var form = new MultipartFormDataContent
            {
                { new StringContent("Test1"), "ProductName" },
                { new StringContent("Description"), "Description" },
                { new StringContent("12"), "ShoppingPrice" },
                { new StringContent("14"), "SellingPrice" },
                { new StringContent("PLN"), "Currency" },
                { new StringContent("Category"), "CategoryName" }
            };

            var client = _factory.CreateClient();
            var resp = await client.PutAsync("api/Product/999", form);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);  
            body.Should().NotBeNullOrEmpty();
            body.Should().Contain("Couldn't find a product");
        }

        [Fact]
        public async Task UpdateProduct_ShouldReturnNotFound_WhenCategoryDoesNotExist()
        {
            int productId;

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
                productId = db.Products.First().ProductId;
            }

            var form = new MultipartFormDataContent
            {
                { new StringContent("NewName"), "ProductName" },
                { new StringContent("Desc"), "Description" },
                { new StringContent("10"), "ShoppingPrice" },
                { new StringContent("20"), "SellingPrice" },
                { new StringContent("PLN"), "Currency" },
                { new StringContent("NOT_EXISTING_CATEGORY"), "CategoryName" }
            };

            var client = _factory.CreateClient();
            var resp = await client.PutAsync($"api/Product/{productId}", form);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.NotFound, body);
            body.Should().Contain("Coundn't find a category");
        }

        [Fact]
        public async Task UpdateProduct_ShouldReturnBadRequest_WhenFileIsNotImage()
        {
            int productId;

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
                productId = db.Products.First().ProductId;
            }

            var form = new MultipartFormDataContent
            {
                { new StringContent("Test1"), "ProductName" },
                { new StringContent("Desc"), "Description" },
                { new StringContent("12"), "ShoppingPrice" },
                { new StringContent("14"), "SellingPrice" },
                { new StringContent("PLN"), "Currency" },
                { new StringContent("Category"), "CategoryName" }
            };

            var bytes = Encoding.UTF8.GetBytes("not an image");
            var file = new ByteArrayContent(bytes);
            file.Headers.ContentType = new MediaTypeHeaderValue("text/plain");

            form.Add(file, "Image", "test.txt");

            var client = _factory.CreateClient();
            var resp = await client.PutAsync($"api/Product/{productId}", form);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
            body.Should().Contain("Only image file are allowed");
        }
        [Fact]
        public async Task UpdateProduct_ShouldReturnBadRequest_WhenImageTooLarge()
        {
            int productId;

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
                productId = db.Products.First().ProductId;
            }

            var form = new MultipartFormDataContent
            {
                { new StringContent("Test1"), "ProductName" },
                { new StringContent("Desc"), "Description" },
                { new StringContent("12"), "ShoppingPrice" },
                { new StringContent("14"), "SellingPrice" },
                { new StringContent("PLN"), "Currency" },
                { new StringContent("Category"), "CategoryName" }
            };

            var bigBytes = new byte[6 * 1024 * 1024]; 
            var bigFile = new ByteArrayContent(bigBytes);
            bigFile.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

            form.Add(bigFile, "Image", "big.jpg");

            var client = _factory.CreateClient();
            var resp = await client.PutAsync($"api/Product/{productId}", form);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
            body.Should().Contain("Image too large");
        }
    }
}
