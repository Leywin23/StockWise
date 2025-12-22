using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Writers;
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

namespace StockWise.Tests.Api.Controllers.AccountController_Tests
{
    public class AccountController_VerifyEmailTest : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;

        public AccountController_VerifyEmailTest(CustomWebAppFactory factory)
        {
            _factory = factory;
        }

        private static async Task CreateUnconfirmedUserAsync(CustomWebAppFactory factory, string email, string code)
        {
            using var scope = factory.Services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();

            var testUser = new AppUser
            {
                UserName = email.Split('@')[0],
                Email = email,
                EmailConfirmed = false,
                CompanyMembershipStatus = CompanyMembershipStatus.Pending
            };

            var res = await userManager.CreateAsync(testUser, "Pas$word1");

            if (!res.Succeeded)
            {
                throw new Exception("Failed to create test user: " +
                    string.Join(", ", res.Errors.Select(e => e.Description)));
            }

            cache.Set(email, code, TimeSpan.FromMinutes(10));
        }

        [Fact]
        public async Task VerifyEmail_ShouldReturnOk()
        {
            var code = "ABCDEF";
            var email = "testMail@test.com";


            await CreateUnconfirmedUserAsync(_factory, email, code);

            var client = _factory.CreateClient();
            var resp = await client.PostAsJsonAsync($"api/Account/verify-email?email", new { Email = email, Code=code});
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
                var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                var testUser = await um.FindByEmailAsync(email);
                testUser?.EmailConfirmed.Should().Be(true);
            }
        }

        [Fact]
        public async Task VerifyEmail_ShouldReturnBadRequestCodeNotFoundOrExpired()
        {
            var code = "ABCDEF";
            var email = "testMail2@test.com";
            var wrongCode = "FEDCBA";

            await CreateUnconfirmedUserAsync(_factory, email, code);

            var client = _factory.CreateClient();

            var resp = await client.PostAsJsonAsync(
                "/api/Account/verify-email",
                new { Email = email, Code = wrongCode }   
            );

            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);

            using (var scope = _factory.Services.CreateScope())
            {
                var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                var testUser = await um.FindByEmailAsync(email);
                testUser.Should().NotBeNull();
                testUser!.EmailConfirmed.Should().BeFalse();
            }

            body.Should().NotBeNullOrEmpty();
        }


    }
}
