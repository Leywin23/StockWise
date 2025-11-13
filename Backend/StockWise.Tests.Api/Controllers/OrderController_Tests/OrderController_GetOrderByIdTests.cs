using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
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

namespace StockWise.Tests.Api.Controllers.OrderController_Tests
{
    public class OrderController_GetOrderByIdTests : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        public OrderController_GetOrderByIdTests(CustomWebAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetOrder_ShouldReturnOk()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("api/Order/1");
            var body = await resp.Content.ReadAsStringAsync();
            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);
        }

        [Fact]
        public async Task GetOrder_ShouldReturnNotFound_ForOrderNotBelongingToUsersCompany()
        {
            int foreignOrderId = 0;

            var wf = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    using var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();

                    var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
                    var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

                    var otherSeller = new Company { Name = "Other", NIP = "1111111111", Address = "x", Email = "o@o.o", Phone = "111111111" };
                    var otherBuyer = new Company { Name = "OtherB", NIP = "2222222222", Address = "y", Email = "b@b.b", Phone = "222222222" };
                    db.Companies.AddRange(otherSeller, otherBuyer);

                    var cat = new Category { Name = "X" };
                    db.Categories.Add(cat);

                    var prod = new CompanyProduct
                    {
                        CompanyProductName = "P",
                        EAN = "12345678",
                        Description = "d",
                        Price = Money.Of(1, "PLN"),
                        Stock = 1,
                        Company = otherSeller,
                        Category = cat,
                        IsAvailableForOrder = true
                    };
                    db.CompanyProducts.Add(prod);
                    db.SaveChanges();

                    var foreignOrder = new Order
                    {
                        Seller = otherSeller,
                        Buyer = otherBuyer,
                        Status = OrderStatus.Pending,
                        CreatedAt = DateTime.UtcNow,
                        UserNameWhoMadeOrder = "someone",
                        ProductsWithQuantity = new List<OrderProduct>
                    {
                        new OrderProduct { CompanyProductId = prod.CompanyProductId, Quantity = 1 }
                    },
                            TotalPrice = Money.Of(1, "PLN")
                    };
                    db.Orders.Add(foreignOrder);
                    db.SaveChanges();

                    foreignOrderId = foreignOrder.Id;
                });
            });

            var client = _factory.CreateClient();
            var resp = await client.GetAsync($"api/Order/{foreignOrderId}");
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.NotFound, body);
        }

        [Fact]
        public async Task GetOrder_ById_WhenNotExists_Returns404()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("/api/Order/999999");
            var body = await resp.Content.ReadAsStringAsync();
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound, body);
        }
    }
}
