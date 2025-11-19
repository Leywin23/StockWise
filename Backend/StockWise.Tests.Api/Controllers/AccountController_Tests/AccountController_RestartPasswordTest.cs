using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
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
    public class AccountController_RestartPasswordTest : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        public AccountController_RestartPasswordTest(CustomWebAppFactory factory)
        {
            _factory = factory;
        }

        private static async Task CreateUserWithResetCode(CustomWebAppFactory factory, string email, string code)
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
            var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();

            var testUser = new AppUser
            {
                UserName = email.Split('@')[0],
                Email = email,
                EmailConfirmed = false,
                CompanyMembershipStatus = CompanyMembershipStatus.Pending
            };

            var res = await um.CreateAsync(testUser, "Pas$word1");

            if (!res.Succeeded)
            {
                throw new Exception("Failed to create test user: " +
                    string.Join(", ", res.Errors.Select(e => e.Description)));
            }

            // 👈 KLUCZ ZGODNY Z GetResetKey(email)
            var cacheKey = $"pwdreset:{email}";

            cache.Set(cacheKey, code, TimeSpan.FromMinutes(10));
        }

        [Fact]
        public async Task RestartPassword_ShouldReturnOkAndChangePassword()
        {
            string email = "reset@test.com";
            string resetCode = "999777";
            string newPassword = "NewPass123!";

            await CreateUserWithResetCode(_factory, email, resetCode);

            var client = _factory.CreateClient();

            var url = $"/api/Account/Restart-Password?email={email}&code={resetCode}&newPassword={newPassword}";

            var resp = await client.PostAsync(url, null);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);
            body.Should().Contain("Password has been reset successfully.");

            using var scope = _factory.Services.CreateScope();
            var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var user = await um.FindByEmailAsync(email);

            (await um.CheckPasswordAsync(user!, newPassword)).Should().BeTrue();
        }

        [Fact]
        public async Task RestartPassword_WithInvalidCode_ShouldReturnBadRequest_AndNotChangePassword()
        {
            string email = "reset-invalid@test.com";
            string correctCode = "123456";
            string wrongCode = "000000";
            string newPassword = "NewPass123!";

            await CreateUserWithResetCode(_factory, email, correctCode);

            var client = _factory.CreateClient();

            var url = $"/api/Account/Restart-Password?email={email}&code={wrongCode}&newPassword={newPassword}";

            var resp = await client.PostAsync(url, null);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
            body.Should().Contain("Invalid verification code.");

            using var scope = _factory.Services.CreateScope();
            var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var user = await um.FindByEmailAsync(email);

            (await um.CheckPasswordAsync(user!, "Pas$word1")).Should().BeTrue();
            (await um.CheckPasswordAsync(user!, newPassword)).Should().BeFalse();
        }

        [Fact]
        public async Task RestartPassword_WithoutCodeInCache_ShouldReturnBadRequest()
        {
            string email = "nocode@test.com";
            string newPassword = "NewPass123!";

            using (var scope = _factory.Services.CreateScope())
            {
                var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

                var user = new AppUser
                {
                    UserName = "nocode",
                    Email = email,
                    EmailConfirmed = false,
                    CompanyMembershipStatus = CompanyMembershipStatus.Pending
                };

                var res = await um.CreateAsync(user, "Pas$word1");
                res.Succeeded.Should().BeTrue();
            }

            var client = _factory.CreateClient();
            var url = $"/api/Account/Restart-Password?email={email}&code=111111&newPassword={newPassword}";

            var resp = await client.PostAsync(url, null);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
            body.Should().Contain("Verification code not found or expired.");
        }

        [Fact]
        public async Task RestartPassword_WithUnknownEmail_ShouldReturnNotFound()
        {
            string email = "doesnotexist@test.com";
            string resetCode = "123456";
            string newPassword = "NewPass123!";

            var client = _factory.CreateClient();
            var url = $"/api/Account/Restart-Password?email={email}&code={resetCode}&newPassword={newPassword}";

            var resp = await client.PostAsync(url, null);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.NotFound, body);
            body.Should().Contain("Email isn't assigned to any account");
        }

        [Fact]
        public async Task RestartPassword_CodeShouldBeSingleUse()
        {
            string email = "singleuse@test.com";
            string resetCode = "555555";
            string firstPassword = "FirstPass123!";
            string secondPassword = "SecondPass123!";

            await CreateUserWithResetCode(_factory, email, resetCode);

            var client = _factory.CreateClient();

            var url1 = $"/api/Account/Restart-Password?email={email}&code={resetCode}&newPassword={firstPassword}";
            var url2 = $"/api/Account/Restart-Password?email={email}&code={resetCode}&newPassword={secondPassword}";

            var resp1 = await client.PostAsync(url1, null);
            var body1 = await resp1.Content.ReadAsStringAsync();

            resp1.StatusCode.Should().Be(HttpStatusCode.OK, body1);

            var resp2 = await client.PostAsync(url2, null);
            var body2 = await resp2.Content.ReadAsStringAsync();

            resp2.StatusCode.Should().Be(HttpStatusCode.BadRequest, body2);
            body2.Should().Contain("Verification code not found or expired.");

            using var scope = _factory.Services.CreateScope();
            var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var user = await um.FindByEmailAsync(email);

            (await um.CheckPasswordAsync(user!, firstPassword)).Should().BeTrue();
            (await um.CheckPasswordAsync(user!, secondPassword)).Should().BeFalse();
        }
    }
}
