using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StockWise.Application.Interfaces;
using StockWise.Infrastructure.Persistence;
using StockWise.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StockWise.Tests.Api.Controllers.AccountController_Tests
{
    public class AccountController_ChangeUserCompanyTest : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;

        public AccountController_ChangeUserCompanyTest(CustomWebAppFactory factory)
        {
            _factory = factory;
        }

        private HttpClient CreateJwtClient(out IServiceProvider sp)
        {
            var wf = _factory.WithWebHostBuilder(b =>
            {
                b.ConfigureTestServices(services =>
                {
                    services.PostConfigureAll<AuthenticationOptions>(o =>
                    {
                        o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    });
                });
            });

            sp = wf.Services;
            return wf.CreateClient();
        }

        private static void SetBearer(HttpClient client, string token) =>
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        private static async Task<string> IssueJwtAsync(IServiceProvider sp, string email = "loginuser@test.com")
        {
            using var scope = sp.CreateScope();
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            var user = await userManager.FindByEmailAsync(email);
            user.Should().NotBeNull("seed w CustomWebAppFactory tworzy loginuser@test.com jako Workera");

            var jwt = await tokenService.CreateToken(user!);
            jwt.Should().NotBeNullOrWhiteSpace();
            return jwt!;
        }
        [Fact]
        public async Task ChangeUserCompany_ShouldReturnOk_AndUpdateCompanyAndStatus()
        {
            var client = CreateJwtClient(out var sp);
            var token = await IssueJwtAsync(sp);
            SetBearer(client, token);

            int? currentCompanyId;
            int targetCompanyId;
            string targetNip;

            using (var scope = sp.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
                var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

                var user = await um.FindByEmailAsync("loginuser@test.com");
                user.Should().NotBeNull();
                currentCompanyId = user!.CompanyId;
                currentCompanyId.Should().NotBeNull();

                var targetCompany = db.Companies.First(c => c.Id != currentCompanyId.Value);
                targetCompanyId = targetCompany.Id;
                targetNip = targetCompany.NIP;
            }

   
            var resp = await client.PostAsync($"/api/Account/companies/change/{targetNip}", null);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);
            body.Should().Contain("has changed company");

            using (var scope2 = sp.CreateScope())
            {
                var um2 = scope2.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                var user2 = await um2.FindByEmailAsync("loginuser@test.com");
                user2.Should().NotBeNull();

                user2!.CompanyId.Should().Be(targetCompanyId);
                user2.CompanyMembershipStatus.Should().Be(CompanyMembershipStatus.Pending);
            }
        }

        [Fact]
        public async Task ChangeUserCompany_ShouldReturnNotFound_WhenCompanyWithNipDoesNotExist()
        {
            var client = CreateJwtClient(out var sp);
            var token = await IssueJwtAsync(sp);
            SetBearer(client, token);

            const string nonExistingNip = "0000000000";

            var resp = await client.PostAsync($"/api/Account/companies/change/{nonExistingNip}", null);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.NotFound, body);
            body.Should().Contain("Company with this NIP not found.");
        }

        [Fact]
        public async Task ChangeUserCompany_ShouldReturnBadRequest_WhenAlreadyAssignedToThisCompany()
        {
            var client = CreateJwtClient(out var sp);
            var token = await IssueJwtAsync(sp);
            SetBearer(client, token);

            string currentNip;

            using (var scope = sp.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
                var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

                var user = await um.FindByEmailAsync("loginuser@test.com");
                user.Should().NotBeNull();
                user!.CompanyId.Should().NotBeNull();

                var company = db.Companies.Single(c => c.Id == user.CompanyId);
                currentNip = company.NIP;
            }

            var resp = await client.PostAsync($"/api/Account/companies/change/{currentNip}", null);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
            body.Should().Contain("You are already assigned to this company.");
        }
    }
}
