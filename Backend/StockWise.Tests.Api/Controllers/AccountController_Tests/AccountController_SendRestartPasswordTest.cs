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

namespace StockWise.Tests.Api.Controllers.AccountController_Tests
{
    public class AccountController_SendRestartPasswordTest : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        public AccountController_SendRestartPasswordTest(CustomWebAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task SendRestartPassword_ShouldReturnOk()
        {
            var email = "Test123@Test.com";
            var userName = "Test";
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
                var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                var user = new AppUser
                {
                    UserName = userName,
                    Email = email,
                    CompanyMembershipStatus = CompanyMembershipStatus.Approved,
                };
                var res = await um.CreateAsync(user, "Pas$word1");
                res.Succeeded.Should().BeTrue();
            }

            var client = _factory.CreateClient();
            var resp = await client.PostAsync($"api/Account/send-reset-password?email={email}", null);
            var body = await resp.Content.ReadAsStringAsync();
            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);
        }

    }
}
