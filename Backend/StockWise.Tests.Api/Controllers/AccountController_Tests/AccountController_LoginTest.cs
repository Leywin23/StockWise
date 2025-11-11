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
    public class AccountController_LoginTest : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;

        public AccountController_LoginTest(CustomWebAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task LoginTest_ShouldReturnOk__WhenCredentialsAreValid()
        {
            var client = _factory.CreateClient();

            var LoginDto = new LoginDto
            {
                Email = "loginuser@test.com",
                Password = "Password123!"
            };

            var resp = await client.PostAsJsonAsync("api/Account/login", LoginDto);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);
        }
        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenEmailDoesNotExist()
        {
            var client = _factory.CreateClient();

            var dto = new LoginDto
            {
                Email = "nonexistent@test.com",
                Password = "Password123!"
            };

            var resp = await client.PostAsJsonAsync("api/Account/login", dto);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized, body);
        }
        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsInvalid()
        {
            var client = _factory.CreateClient();

            var dto = new LoginDto
            {
                Email = "loginuser@test.com",
                Password = "WrongPassword!"
            };

            var resp = await client.PostAsJsonAsync("api/Account/login", dto);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized, body);
        }
    }
}
