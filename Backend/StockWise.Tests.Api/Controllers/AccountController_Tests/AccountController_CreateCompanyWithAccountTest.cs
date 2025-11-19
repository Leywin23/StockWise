using StockWise.Application.Contracts.AccountDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using StockWise.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using StockWise.Models;

namespace StockWise.Tests.Api.Controllers.AccountController_Tests
{
    public class AccountController_CreateCompanyWithAccountTest : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        public AccountController_CreateCompanyWithAccountTest(CustomWebAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CreteCompanyWithAccountTest_ShouldReturnOkAndCreateCompanyAndUser()
        {
            var client = _factory.CreateClient();

            var dto = new CreateCompanyWithAccountDto
            {
                UserName = "TestUser123",
                Email = "TestEmail123@Test.com",
                Password = "Pas$word1",
                CompanyName = "TestCompany123",
                NIP = "7986542340",
                Address = "Testowa123",
                CompanyEmail = "TestEmail123@Test.com",
                Phone = "132321321"
            };

            var resp = await client.PostAsJsonAsync("api/Account/CompanyWithUser", dto);

            var body = await resp.Content.ReadAsStringAsync();
            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
                var company = await db.Companies.FirstOrDefaultAsync(c => c.Email == dto.Email);
                company.Should().NotBeNull();
                company.Email.Should().Be(dto.Email);
                company.Name.Should().Be(dto.CompanyName);
                company.NIP.Should().Be(dto.NIP);
                company.Email.Should().Be(dto.Email);
                company.Address.Should().Be(dto.Address);
                var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                var user = await um.FindByEmailAsync(dto.Email);
                user.Should().NotBeNull();
                user.UserName.Should().Be(dto.UserName);
                user.Email.Should().Be(dto.Email);
                user.Company.Should().Be(company);
            }
        }

        [Fact]
        public async Task CreateCompanyWithAccount_ShouldReturnConflict_WhenCompanyWithNipAlreadyExists()
        {
            var client = _factory.CreateClient();

            var dto = new CreateCompanyWithAccountDto
            {
                UserName = "NewUserForNip",
                Email = "newuserfornip@test.com",
                Password = "Pas$word1",
                CompanyName = "SomeNewCompany",
                NIP = "1234567890",           
                Address = "Testowa 1",
                CompanyEmail = "newcompany@test.com",
                Phone = "111222333"
            };

            var resp = await client.PostAsJsonAsync("api/Account/CompanyWithUser", dto);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.Conflict, body);
            body.Should().Contain("Comapny with this NIP already exist");
        }

        [Fact]
        public async Task CreateCompanyWithAccount_ShouldReturnConflict_WhenCompanyWithNameAlreadyExists()
        {
            var client = _factory.CreateClient();

            var dto = new CreateCompanyWithAccountDto
            {
                UserName = "NewUserForName",
                Email = "newuserforname@test.com",
                Password = "Pas$word1",
                CompanyName = "ACME",           
                NIP = "1112223334",             
                Address = "Testowa 2",
                CompanyEmail = "newcompanyname@test.com",
                Phone = "444555666"
            };

            var resp = await client.PostAsJsonAsync("api/Account/CompanyWithUser", dto);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.Conflict, body);
            body.Should().Contain("Comapny with this name already exist");
        }

        [Fact]
        public async Task CreateCompanyWithAccount_ShouldReturnConflict_WhenUserNameAlreadyExists()
        {
            var client = _factory.CreateClient();

            var dto = new CreateCompanyWithAccountDto
            {
                UserName = "loginuser",          
                Email = "user_with_taken_name@test.com",
                Password = "Pas$word1",
                CompanyName = "UniqueCompanyForUsername",
                NIP = "2223334445",
                Address = "Testowa 3",
                CompanyEmail = "company_for_username@test.com",
                Phone = "777888999"
            };

            var resp = await client.PostAsJsonAsync("api/Account/CompanyWithUser", dto);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.Conflict, body);
            body.Should().Contain("User with this name already exist");
        }

        [Fact]
        public async Task CreateCompanyWithAccount_ShouldReturnConflict_WhenEmailAlreadyExists()
        {
            var client = _factory.CreateClient();

            var dto = new CreateCompanyWithAccountDto
            {
                UserName = "UserWithTakenEmail",
                Email = "loginuser@test.com",    
                Password = "Pas$word1",
                CompanyName = "UniqueCompanyForEmail",
                NIP = "3334445556",
                Address = "Testowa 4",
                CompanyEmail = "company_for_email@test.com",
                Phone = "123123123"
            };

            var resp = await client.PostAsJsonAsync("api/Account/CompanyWithUser", dto);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.Conflict, body);
            body.Should().Contain("User with this email already exist");
        }

    }
}
