using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using StockWise.Infrastructure.Persistence;
using StockWise.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StockWise.Tests.Api.Controllers.OrderController_Tests
{
    public class OrderController_DeleteOrderTest : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        public OrderController_DeleteOrderTest(CustomWebAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task DeleteOrder_ShouldReturnOk_AndRemoveFromDb()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();

            var client = _factory.CreateClient();
            var resp = await client.DeleteAsync($"/api/Order/1");
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);
            db.Orders.Find(1).Should().BeNull();
        }

        [Fact]
        public async Task DeleteOrder_CannotDeleteOrderOfAnotherCompany()
        {
            var foreignOrderId = 0;
            using(var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
                var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

                var otherSeller = new Company { Name = "TestSeller", Address = "Test 11", Email = "Test@test.test", NIP = "1112223330", Phone = "111222333" };
                var otherBuyer = new Company { Name = "TestBuyer", Address = "Test 12", Email = "Test2@test.test", NIP = "0333222111", Phone = "333222111" };
                db.Companies.AddRange(otherBuyer, otherSeller);
                db.SaveChanges();

                var testCategory = new Category { Name = "Test" };
                db.Categories.Add(testCategory);
                var SellerProductForOrder = new CompanyProduct { CompanyProductName = "TestProductToOrder", EAN = "54321098", Category = testCategory, Description = "Test", Company = otherSeller, Stock = 999, Price = Money.Of(12, "EUR") };
                db.CompanyProducts.Add(SellerProductForOrder);
                db.SaveChanges();
                var testOrder = new Order
                {
                    Buyer = otherBuyer,
                    Seller = otherSeller,
                    Status = OrderStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UserNameWhoMadeOrder = "XXX"
                };
                var orderItem = new OrderProduct
                {
                    CompanyProductId = SellerProductForOrder.CompanyProductId,
                    Quantity = 99,
                };
                testOrder.ProductsWithQuantity.Add(orderItem);
                testOrder.TotalPrice = Money.Of(SellerProductForOrder.Price.Amount * 99, SellerProductForOrder.Price.Currency.Code);
                db.Orders.Add(testOrder);
                db.SaveChanges();
                foreignOrderId = testOrder.Id;
            }
            var client = _factory.CreateClient();
            var resp = await client.DeleteAsync($"api/Order/{foreignOrderId}");
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized, body);
            body.Should().Contain("You cannot delete orders of another company.");
            using (var scope2 = _factory.Services.CreateScope())
            {
                var db2 = scope2.ServiceProvider.GetRequiredService<StockWiseDb>();
                db2.Orders.Find(foreignOrderId)
                    .Should().NotBeNull("order belongs to another company and must not be deleted");
            }
        }

        [Fact]
        public async Task DeleteOrder_ShouldReturnNotFound()
        {
            int id = 999;
            var client = _factory.CreateClient();
            var resp = await client.DeleteAsync($"api/Order/{id}");
            var body = await resp.Content.ReadAsStringAsync();
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound, body);
            body.Should().Contain($"Order with id: {id} not found");
        }

        
    }
}
