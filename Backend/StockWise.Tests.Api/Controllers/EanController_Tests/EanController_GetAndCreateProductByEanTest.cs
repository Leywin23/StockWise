using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using StockWise.Infrastructure.Persistence;
using StockWise.Models;
using StockWise.Tests.Api.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace StockWise.Tests.Api.Controllers.EanController_Tests
{
    public class EanController_GetAndCreateProductByEanTest : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        public EanController_GetAndCreateProductByEanTest(CustomWebAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetProductByEan_ShouldReturnOk_AndCreateProduct()
        {
            var ean = "1234567890123";

            var fakeJson = """
            {
              "items": [
                {
                  "title": "API Test Product",
                  "category": "Electronics",
                  "description": "API description",
                  "images": ["https://example.com/img.png"],
                  "lowest_recorded_price": 5.40,
                  "highest_recorded_price": 9.99
                }
              ]
            }
            """;

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(fakeJson, Encoding.UTF8, "application/json")
            };

            var fakeHandler = new FakeHttpMessageHandler(httpResponse);
            var fakeHttpClient = new HttpClient(fakeHandler);

            var factory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IHttpClientFactory));

                    var httpFactoryMock = new Mock<IHttpClientFactory>();
                    httpFactoryMock
                        .Setup(x => x.CreateClient(It.IsAny<string>()))
                        .Returns(fakeHttpClient);

                    services.AddSingleton<IHttpClientFactory>(httpFactoryMock.Object);
                });
            });

            var client = factory.CreateClient();

            var response = await client.GetAsync($"api/Ean/{ean}");
            var body = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK, body);

            var result = JsonConvert.DeserializeObject<Product>(body);
            result.Should().NotBeNull();

            result!.ProductName.Should().Be("API Test Product");
            result.Description.Should().Be("API description");
            result.EAN.Should().Be(ean);
            result.Image.Should().Be("https://example.com/img.png");
            result.ShoppingPrice.Amount.Should().Be(5.40m);
            result.SellingPrice.Amount.Should().Be(9.99m);

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();

            var inDb = db.Products.FirstOrDefault(p => p.EAN == ean);
            inDb.Should().NotBeNull();
            inDb!.ProductName.Should().Be("API Test Product");
        }

        [Fact]
        public async Task GetProductByEan_ShouldReturnBadRequest_WhenExternalApiFails()
        {
            var ean = "1234567890123";

            var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            var fakeHandler = new FakeHttpMessageHandler(httpResponse);
            var fakeHttpClient = new HttpClient(fakeHandler);

            var factory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var httpFactoryMock = new Mock<IHttpClientFactory>();
                    httpFactoryMock
                        .Setup(x => x.CreateClient(It.IsAny<string>()))
                        .Returns(fakeHttpClient);

                    services.AddSingleton<IHttpClientFactory>(httpFactoryMock.Object);
                });
            });

            var client = factory.CreateClient();

            var response = await client.GetAsync($"api/Ean/{ean}");
            var body = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
            body.Should().Contain("API resolution error");
        }

        [Fact]
        public async Task GetProductByEan_ShouldReturnNotFound_WhenItemsArrayIsEmpty()
        {
            var ean = "1234567890123";

            var fakeJson = """{ "items": [] }""";

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(fakeJson, Encoding.UTF8, "application/json")
            };

            var fakeHandler = new FakeHttpMessageHandler(httpResponse);
            var fakeHttpClient = new HttpClient(fakeHandler);

            var factory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var httpFactoryMock = new Mock<IHttpClientFactory>();
                    httpFactoryMock
                        .Setup(x => x.CreateClient(It.IsAny<string>()))
                        .Returns(fakeHttpClient);

                    services.AddSingleton<IHttpClientFactory>(httpFactoryMock.Object);
                });
            });

            var client = factory.CreateClient();

            var response = await client.GetAsync($"api/Ean/{ean}");
            var body = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.NotFound, body);
            body.Should().Contain("Product with EAN");
            body.Should().Contain(ean);
        }
    }

}

