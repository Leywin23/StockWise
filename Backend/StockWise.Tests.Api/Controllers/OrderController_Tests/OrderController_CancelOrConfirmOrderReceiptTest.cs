using FluentAssertions;
using Microsoft.AspNetCore.Identity;
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
    public class OrderController_CancelOrConfirmOrderReceiptTest : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;

        public OrderController_CancelOrConfirmOrderReceiptTest(CustomWebAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CancellOrCorfirm_ConfirmShouldReturnOk_AndUpdateStatus()
        {
            int orderId;

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();

                var buyer = db.Companies.Single(c => c.NIP == "1234567890"); 
                var seller = db.Companies.First(c => c.NIP != "1234567890");

                var cat = db.Categories.FirstOrDefault() ?? new Category { Name = "CancelConfirm" };
                if (cat.CategoryId == 0)
                {
                    db.Categories.Add(cat);
                    db.SaveChanges();
                }

                var prod = new CompanyProduct
                {
                    CompanyProductName = "SellerProd",
                    EAN = "87654321",
                    Description = "Test",
                    Price = Money.Of(10m, "PLN"),
                    Stock = 100,
                    Company = seller,
                    Category = cat,
                    IsAvailableForOrder = true
                };
                db.CompanyProducts.Add(prod);
                db.SaveChanges();

                var order = new Order
                {
                    Seller = seller,
                    Buyer = buyer,
                    Status = OrderStatus.Accepted,
                    CreatedAt = DateTime.UtcNow,
                    UserNameWhoMadeOrder = "loginuser",
                    TotalPrice = Money.Of(10m * 5, "PLN"),
                    ProductsWithQuantity = new List<OrderProduct>
                    {
                        new OrderProduct
                        {
                            CompanyProductId = prod.CompanyProductId,
                            Quantity = 5
                        }
                    }
                };

                db.Orders.Add(order);
                db.SaveChanges();

                orderId = order.Id;
            }

            var client = _factory.CreateClient();

            var resp = await client.PutAsJsonAsync(
                $"/api/Order/CancellOrCorfirm/{orderId}",
                OrderStatus.Completed); 

            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);

            using (var scope2 = _factory.Services.CreateScope())
            {
                var db2 = scope2.ServiceProvider.GetRequiredService<StockWiseDb>();
                var updated = db2.Orders.Single(o => o.Id == orderId);
                updated.Status.Should().Be(OrderStatus.Completed);
            }
        }

        [Fact]
        public async Task CancellOrCorfirm_CancelShouldReturnOk_AndUpdateStatus()
        {
            int orderId;

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();

                var buyer = db.Companies.Single(c => c.NIP == "1234567890");
                var seller = db.Companies.First(c => c.NIP != "1234567890");

                var cat = db.Categories.FirstOrDefault() ?? new Category { Name = "CancelConfirm2" };
                if (cat.CategoryId == 0)
                {
                    db.Categories.Add(cat);
                    db.SaveChanges();
                }

                var prod = new CompanyProduct
                {
                    CompanyProductName = "SellerProd2",
                    EAN = "12344321",
                    Description = "Test2",
                    Price = Money.Of(15m, "PLN"),
                    Stock = 50,
                    Company = seller,
                    Category = cat,
                    IsAvailableForOrder = true
                };
                db.CompanyProducts.Add(prod);
                db.SaveChanges();

                var order = new Order
                {
                    Seller = seller,
                    Buyer = buyer,
                    Status = OrderStatus.Accepted,
                    CreatedAt = DateTime.UtcNow,
                    UserNameWhoMadeOrder = "loginuser",
                    TotalPrice = Money.Of(15m * 2, "PLN"),
                    ProductsWithQuantity = new List<OrderProduct>
                    {
                        new OrderProduct
                        {
                            CompanyProductId = prod.CompanyProductId,
                            Quantity = 2
                        }
                    }
                };

                db.Orders.Add(order);
                db.SaveChanges();

                orderId = order.Id;
            }

            var client = _factory.CreateClient();

            var resp = await client.PutAsJsonAsync(
                $"/api/Order/CancellOrCorfirm/{orderId}",
                OrderStatus.Cancelled);

            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);

            using (var scope2 = _factory.Services.CreateScope())
            {
                var db2 = scope2.ServiceProvider.GetRequiredService<StockWiseDb>();
                var updated = db2.Orders.Single(o => o.Id == orderId);
                updated.Status.Should().Be(OrderStatus.Cancelled);
            }
        }

        [Fact]
        public async Task CancellOrCorfirm_InvalidStatus_ShouldReturnBadRequest()
        {
            int orderId;

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();

                var buyer = db.Companies.Single(c => c.NIP == "1234567890");
                var seller = db.Companies.First(c => c.NIP != "1234567890");

                var cat = db.Categories.FirstOrDefault() ?? new Category { Name = "CancelConfirmInvalid" };
                if (cat.CategoryId == 0)
                {
                    db.Categories.Add(cat);
                    db.SaveChanges();
                }

                var prod = new CompanyProduct
                {
                    CompanyProductName = "SellerProd3",
                    EAN = "99998888",
                    Description = "Test3",
                    Price = Money.Of(20m, "PLN"),
                    Stock = 30,
                    Company = seller,
                    Category = cat,
                    IsAvailableForOrder = true
                };
                db.CompanyProducts.Add(prod);
                db.SaveChanges();

                var order = new Order
                {
                    Seller = seller,
                    Buyer = buyer,
                    Status = OrderStatus.Accepted,
                    CreatedAt = DateTime.UtcNow,
                    UserNameWhoMadeOrder = "loginuser",
                    TotalPrice = Money.Of(20m, "PLN"),
                    ProductsWithQuantity = new List<OrderProduct>
                    {
                        new OrderProduct
                        {
                            CompanyProductId = prod.CompanyProductId,
                            Quantity = 1
                        }
                    }
                };

                db.Orders.Add(order);
                db.SaveChanges();

                orderId = order.Id;
            }

            var client = _factory.CreateClient();

            var resp = await client.PutAsJsonAsync(
                $"/api/Order/CancellOrCorfirm/{orderId}",
                OrderStatus.Pending);

            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
            body.Should().Contain("Unsupported order status");
        }

        [Fact]
        public async Task CancellOrCorfirm_OrderNotFound_ShouldReturn404()
        {
            var client = _factory.CreateClient();

            var resp = await client.PutAsJsonAsync(
                "/api/Order/CancellOrCorfirm/999999",
                OrderStatus.Completed);

            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.NotFound, body);
        }

    }
}
