using FluentAssertions;
using MailKit.Search;
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
    public class OrderController_AcceptOrRejectOrderTest : IClassFixture<CustomWebAppFactory>
    {
        private  readonly CustomWebAppFactory _factory;
        public OrderController_AcceptOrRejectOrderTest(CustomWebAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task AcceptOrRejectOrder_AcceptShouldReturnOk()
        {
            int orderId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
                orderId = db.Orders.First().Id; 
            }
            var client = _factory.CreateClient();
            var status = OrderStatus.Accepted;
            var resp = await client.PutAsJsonAsync($"/api/Order/AcceptOrRejectOrder/{orderId}", status);
            var body = await resp.Content.ReadAsStringAsync();
            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);
            
            using (var scope2 = _factory.Services.CreateScope())
            {
                var db2 = scope2.ServiceProvider.GetRequiredService<StockWiseDb>();
                var updated = db2.Orders.Single(o=> o.Id == orderId);
                updated.Status.Should().Be(OrderStatus.Accepted);
            }
        }
        [Fact]
        public async Task AcceptOrRejectOrder_RejectShouldReturnOk()
        {
            int orderId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
                orderId = db.Orders.First().Id;
            }
            var client = _factory.CreateClient();
            var status = OrderStatus.Rejected;
            var resp = await client.PutAsJsonAsync($"/api/Order/AcceptOrRejectOrder/{orderId}", status);
            var body = await resp.Content.ReadAsStringAsync();
            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);

            using (var scope2 = _factory.Services.CreateScope())
            {
                var db2 = scope2.ServiceProvider.GetRequiredService<StockWiseDb>();
                var updated = db2.Orders.Single(o => o.Id == orderId);
                updated.Status.Should().Be(OrderStatus.Rejected);
            }
        }
        [Fact]
        public async Task AcceptOrRejectOrder_InvalidStatus_ShouldReturnBadRequest()
        {
            int orderId;

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
                orderId = db.Orders.First().Id;
            }

            var client = _factory.CreateClient();

            var resp = await client.PutAsJsonAsync(
                $"/api/Order/AcceptOrRejectOrder/{orderId}",
                OrderStatus.Pending);

            var body = await resp.Content.ReadAsStringAsync();
            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
            body.Should().Contain("Unsupported order status");
        }
        [Fact]
        public async Task AcceptOrRejectOrder_OrderNotFound_ShouldReturn404()
        {
            var client = _factory.CreateClient();

            var resp = await client.PutAsJsonAsync(
                "/api/Order/AcceptOrRejectOrder/999999",
                OrderStatus.Accepted);

            var body = await resp.Content.ReadAsStringAsync();
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound, body);
        }
    }
}
