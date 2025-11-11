using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StockWise.Tests.Api.Controllers.InventoryMovementController_Tests
{
    public class InventoryMovementController_GetProductMovementHistoryTest : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        public InventoryMovementController_GetProductMovementHistoryTest(CustomWebAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetProductMovementHistoryTest_ShouldReturnOk()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("api/InventoryMovement/1");
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);
        }
        [Fact]
        public async Task GetProductMovementHistoryTest_ShouldReturnNotFound()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("api/InventoryMovement/999");
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.NotFound, body);
        }
    }
}
