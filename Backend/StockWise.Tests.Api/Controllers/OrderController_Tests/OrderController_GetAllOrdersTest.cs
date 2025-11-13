using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StockWise.Application.Abstractions;
using StockWise.Application.Contracts.OrderDtos;
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
    public sealed class ServiceResultDto<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }
    public class OrderController_GetAllOrdersTest : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        public OrderController_GetAllOrdersTest(CustomWebAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetAllOrders_ShouldReturnOk()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("api/Order");
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);
        }

        [Fact]
        public async Task GetAllOrders_WhenNoOrdersForCompany_ReturnsEmptyList()
        {
            var wf = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    using var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
                    var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

                    var emptyCo = new Company { Name = "EmptyCo", NIP = "3333333333", Address = "z", Email = "e@e.e", Phone = "3" };
                    db.Companies.Add(emptyCo);
                    db.SaveChanges();

                    var u = new AppUser { UserName = "emptyuser", Email = "empty@test.com", EmailConfirmed = true, CompanyId = emptyCo.Id, CompanyMembershipStatus = CompanyMembershipStatus.Approved };
                    um.CreateAsync(u, "Password123!").GetAwaiter().GetResult();
                });
            });

            var client = wf.CreateClient();

            var resp = await client.GetAsync("api/Order");
            var body = await resp.Content.ReadAsStringAsync();
            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);
        }

        [Fact]
        public async Task GetAllOrders_ShouldReturnOnlyOrdersBelongingToUsersCompany()
        {
            int foreignOrderId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
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
                        new OrderProduct
                        {
                            CompanyProductId = prod.CompanyProductId,
                            Quantity = 1
                        }
                    },
                    TotalPrice = Money.Of(1, "PLN")
                };
                db.Orders.Add(foreignOrder);
                db.SaveChanges();
                foreignOrderId = foreignOrder.Id;
            }

            var client = _factory.CreateClient();
            var resp = await client.GetAsync("api/Order");
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);

            var list = await resp.Content.ReadFromJsonAsync<List<OrderListDto>>();

            list.Should().NotBeNull();
            list!.Should().OnlyContain(o => o.Seller.NIP == "1234567890" || o.Buyer.NIP == "1234567890");
            list.Select(o => o.Id).Should().NotContain(foreignOrderId);
        }
    }
}
