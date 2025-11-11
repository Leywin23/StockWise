using FluentAssertions;
using StockWise.Application.Contracts.AccountDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StockWise.Tests.Api.Controllers.AccountController_Tests
{
    public class AccountController_RegisterTest : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        public AccountController_RegisterTest(CustomWebAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Register_ShouldReturnOk()
        {
            var client = _factory.CreateClient();

            var userDto = new RegisterDto
            {
                CompanyNIP = "1234567890",
                Email = "TestUserEmail@Test.com",
                UserName = "TestUser123",
                Password = "Password123",
            };

            var resp = await client.PostAsJsonAsync("api/Account/register", userDto);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);
        }
    }
}
