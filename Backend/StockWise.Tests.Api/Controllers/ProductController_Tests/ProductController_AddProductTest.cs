using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using StockWise.Application.Contracts.ProductDtos;
using StockWise.Infrastructure.Persistence;
using StockWise.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StockWise.Tests.Api.Controllers.ProductController_Tests
{
    public class ProductController_AddProductTest : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        public ProductController_AddProductTest(CustomWebAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task AddCompanyProduct_ShouldReturnOkAndProductDto()
        {
            var client = _factory.CreateClient();
            var form = new MultipartFormDataContent
            {
                { new StringContent("Test1"), "ProductName" },
                { new StringContent("32143276"), "EAN" },
                { new StringContent("Description"), "Description" },
                { new StringContent("12"), "ShoppingPrice" },
                { new StringContent("14"), "SellingPrice" },
                { new StringContent("PLN"), "Currency" },
                { new StringContent("X"), "Category" }
            };

            var resp = await client.PostAsync("api/Product", form);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
                var result = db.Products.FirstOrDefault(p=>p.EAN == "32143276");
                result.Should().NotBeNull();
                result!.ProductName.Should().Be("Test1");
            }
            body.Should().NotBeNullOrEmpty();

            var productDto = await resp.Content.ReadFromJsonAsync<Product>();
            productDto.Should().NotBeNull();
            productDto!.ProductName.Should().Be("Test1");
            productDto.EAN.Should().Be("32143276");
        }

        [Fact]
        public async Task AddCompanyProduct_ShouldReturnBadRequestProductAlredyAdded()
        {
            var productEan = "";
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
                var result = db.Products.First();
                productEan = result.EAN;
            }

            var client = _factory.CreateClient();
            var form = new MultipartFormDataContent
            {
                { new StringContent("Test1"), "ProductName" },
                { new StringContent($"{productEan}"), "EAN" },
                { new StringContent("Description"), "Description" },
                { new StringContent("12"), "ShoppingPrice" },
                { new StringContent("14"), "SellingPrice" },
                { new StringContent("PLN"), "Currency" },
                { new StringContent("X"), "Category" }
            };

            var resp = await client.PostAsync("api/Product", form);
            var body = await resp.Content.ReadAsStringAsync();
            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
            body.Should().Contain($"Product with EAN {productEan} already added");
        }
        [Fact]
        public async Task AddCompanyProduct_ShouldReturnBadRequestOnlyImagesAreAllowed()
        {
            var client = _factory.CreateClient();

            var form = new MultipartFormDataContent
            {
                { new StringContent("Test1"), "ProductName" },
                { new StringContent("32143276"), "EAN" },
                { new StringContent("Description"), "Description" },
                { new StringContent("12"), "ShoppingPrice" },
                { new StringContent("14"), "SellingPrice" },
                { new StringContent("PLN"), "Currency" },
                { new StringContent("X"), "Category" }
            };

            var fileBytes = Encoding.UTF8.GetBytes("Some content");
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
            form.Add(fileContent, "Image", "test.txt");

            var resp = await client.PostAsync("api/Product", form);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
            body.Should().Contain("Only image files are allowed");
        }
        [Fact]
        public async Task AddCompanyProduct_ShouldReturnBadRequest_WhenRequiredFieldsMissing()
        {
            var client = _factory.CreateClient();

            var form = new MultipartFormDataContent
            {
                { new StringContent("12"), "ShoppingPrice" },
                { new StringContent("14"), "SellingPrice" },
                { new StringContent("PLN"), "Currency" }
            };

            var resp = await client.PostAsync("api/Product", form);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);

            body.Should().Contain("ProductName");
            body.Should().Contain("EAN");
            body.Should().Contain("Description");
            body.Should().Contain("Category");
        }
    }
}
