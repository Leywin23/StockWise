using FluentAssertions;
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
    public class CompanyController_CreateCompanyTest : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        public CompanyController_CreateCompanyTest(CustomWebAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CreateCompany_ShouldReturnOk()
        {
            var client = _factory.CreateClient();

            var payload = new
            {
                Name = "TestCompany",
                NIP = "8765432109",         
                Address = "TestAddress 11",
                Email = "Test321@test.com",
                Phone = "323432321"
            };

            var resp = await client.PostAsJsonAsync("api/Company", payload);

            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);
        }

        [Fact]
        public async Task CreateCompany_ShouldReturnBadRequest_NIPShouldHave10Digits()
        {
            var client = _factory.CreateClient();
            var payload = new
            {
                Name = "TestCompany",
                NIP = "87654321",
                Address = "TestAddress 11",
                Email = "Test321@test.com",
                Phone = "323432321"
            };

            var resp = await client.PostAsJsonAsync("api/Company", payload);

            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
        }

        [Fact]
        public async Task CreateCompany_ShouldReturnConfict_CompanyAlreadyExist()
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

            var resp = await client.PostAsJsonAsync("api/Company", payload);

            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.Conflict, body);
        }
    }
}
