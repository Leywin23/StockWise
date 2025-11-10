using FluentAssertions;
using StockWise.Application.Contracts.CompanyDtos;
using StockWise.Infrastructure.Services;
using StockWise.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StockWise.Tests.Api.Controllers.CompanyController_Tests
{
    public class CompanyController_UpdateCompanyTest : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        public CompanyController_UpdateCompanyTest(CustomWebAppFactory factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData(1)]
        public async Task UpdateCompany_ShouldReturnOk(int id)
        {
            var client = _factory.CreateClient();

            var payload = new
            {
                Name = "UpdatedCompany",
                NIP = "87654321",
                Address = "Updated Address 123",
                Email = "updated@test.com",
                Phone = "987654321"
            };

            var resp = await client.PutAsJsonAsync($"api/Company", payload);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);
        }
        [Fact]
        public async Task UpdateCompany_ShouldReturnConflict_WhenNameAlreadyExists()
        {
            var client = _factory.CreateClient();

            var payload = new 
            {
                Name = "ACME",
                NIP = "1234567890",
                Address = "123 Test Street",
                Email = "acme@test.com",
                Phone = "123456789"
            };

            var resp = await client.PutAsJsonAsync("api/Company", payload);
            var body = await resp.Content.ReadAsStringAsync();
            resp.StatusCode.Should().Be(HttpStatusCode.Conflict, body);
        }
      
    }
}
