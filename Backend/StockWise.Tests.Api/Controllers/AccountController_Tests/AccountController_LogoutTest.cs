using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using StockWise.Application.Interfaces;
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
    public class AccountController_Logout_Jwt_Tests : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;

        public AccountController_Logout_Jwt_Tests(CustomWebAppFactory factory)
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
            user.Should().NotBeNull("seed z CustomWebAppFactory tworzy loginuser@test.com");

            var jwt = await tokenService.CreateToken(user!);
            jwt.Should().NotBeNullOrWhiteSpace();
            return jwt!;
        }

        [Fact]
        public async Task Logout_Should_ReturnOk()
        {
            var client = CreateJwtClient(out var sp);
            var token = await IssueJwtAsync(sp);
            SetBearer(client, token);

            var logout1 = await client.PostAsync("/api/account/logout", null);

            logout1.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Logout_ShouldBeIdempotent_WhenCalledTwice()
        {
            var client = CreateJwtClient(out var sp);

            var token1 = await IssueJwtAsync(sp);
            SetBearer(client, token1);

            var r1 = await client.PostAsync("/api/account/logout", null);
            r1.StatusCode.Should().Be(HttpStatusCode.OK);

            var token2 = await IssueJwtAsync(sp);
            SetBearer(client, token2);

            var r2 = await client.PostAsync("/api/account/logout", null);
            r2.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        [Fact]
        public async Task Logout_WithoutAuthorizationHeader_Returns401()
        {
            var client = CreateJwtClient(out _); 
            var resp = await client.PostAsync("/api/account/logout", null);
            resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

    }
}
