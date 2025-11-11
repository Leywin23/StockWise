using FluentAssertions;
using StockWise.Application.Contracts.InventoryMovementDtos;
using StockWise.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StockWise.Tests.Api.Controllers.InventoryMovementController_Tests
{
    public class InventoryMovementController_AddMovement : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        public InventoryMovementController_AddMovement(CustomWebAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task AddMovement_ShouldReturnOk()
        {
            var client = _factory.CreateClient();
            var inventoryMovement = new InventoryMovementDto
            {
                CompanyProductId = 1,
                Date = DateTime.UtcNow,
                Type = MovementType.Inbound,
                Quantity = 20,
                Comment = "Test"
            };
            var resp = await client.PostAsJsonAsync("api/InventoryMovement", inventoryMovement);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);
        }

        [Fact]
        public async Task AddMovement_ShouldReturnProductNotFound()
        {
            var client = _factory.CreateClient();
            var inventoryMovement = new InventoryMovementDto
            {
                CompanyProductId = 999,
                Date = DateTime.UtcNow,
                Type = MovementType.Inbound,
                Quantity = 20,
                Comment = "Test"
            };
            var resp = await client.PostAsJsonAsync("api/InventoryMovement", inventoryMovement);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.NotFound, body);
        }

        [Fact]
        public async Task AddMovement_ShouldReturnBadRequestProductStockCouldntBeBelowZero()
        {
            var client = _factory.CreateClient();
            var inventoryMovement = new InventoryMovementDto
            {
                CompanyProductId = 1,
                Date = DateTime.UtcNow,
                Type = MovementType.Outbound,
                Quantity = 99999,
                Comment = "Test"
            };
            var resp = await client.PostAsJsonAsync("api/InventoryMovement", inventoryMovement);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
        }
    }
}
