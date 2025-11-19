using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using StockWise.Application.Contracts.AccountDtos;
using StockWise.Models;
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
        public async Task Register_ShouldReturnOk_AndCreateUser()
        {
            var client = _factory.CreateClient();

            var dto = new RegisterDto
            {
                CompanyNIP = "1234567890",               
                Email = "register_ok@test.com",
                UserName = "RegisterUser1",
                Password = "Password123!"
            };

            var resp = await client.PostAsJsonAsync("api/Account/register", dto);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);

            using var scope = _factory.Services.CreateScope();
            var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var user = await um.FindByEmailAsync(dto.Email);

            user.Should().NotBeNull();
            user!.UserName.Should().Be(dto.UserName);
            user.EmailConfirmed.Should().BeFalse(); 
            (await um.IsInRoleAsync(user, "Worker")).Should().BeTrue();
        }

        [Fact]
        public async Task Register_ShouldReturnNotFound_WhenCompanyDoesNotExist()
        {
            var client = _factory.CreateClient();

            var dto = new RegisterDto
            {
                CompanyNIP = "0000000000",         
                Email = "no_company@test.com",
                UserName = "NoCompanyUser",
                Password = "Password123!"
            };

            var resp = await client.PostAsJsonAsync("api/Account/register", dto);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.NotFound, body);
            body.Should().Contain("There isn't any company with NIP");
        }

        [Fact]
        public async Task Register_ShouldReturnConflict_WhenEmailAlreadyExists()
        {
            var client = _factory.CreateClient();

            var dto = new RegisterDto
            {
                CompanyNIP = "1234567890",
                Email = "loginuser@test.com",   
                UserName = "SomeOtherName",
                Password = "Password123!"
            };

            var resp = await client.PostAsJsonAsync("api/Account/register", dto);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.Conflict, body);
            body.Should().Contain("Email already exists");
        }

        [Fact]
        public async Task Register_ShouldReturnConflict_WhenUsernameAlreadyExists()
        {
            var client = _factory.CreateClient();

            var dto = new RegisterDto
            {
                CompanyNIP = "1234567890",
                Email = "unique_for_username@test.com",
                UserName = "loginuser",           
                Password = "Password123!"
            };

            var resp = await client.PostAsJsonAsync("api/Account/register", dto);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.Conflict, body);
            body.Should().Contain("Username already exists");
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenModelIsInvalid()
        {
            var client = _factory.CreateClient();

            var dto = new RegisterDto
            {
                CompanyNIP = "1234567890",          
                Email = "not-an-email",  
                UserName = "",            
                Password = ""            
            };

            var resp = await client.PostAsJsonAsync("api/Account/register", dto);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
        }
    }
}
