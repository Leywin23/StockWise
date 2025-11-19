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

namespace StockWise.Tests.Api.Controllers.CompanyController_Tests
{
    public class CompanyController_DeleteCompanyTest : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        public CompanyController_DeleteCompanyTest(CustomWebAppFactory factory)
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

        private static async Task<string> IssueJwtForUserAsync(IServiceProvider sp, AppUser user)
        {
            using var scope = sp.CreateScope();
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

            var jwt = await tokenService.CreateToken(user);
            jwt.Should().NotBeNullOrWhiteSpace();
            return jwt!;
        }

        [Fact]
        public async Task DeleteCompanyTest_ShouldReturnOk()
        {
            var client = CreateJwtClient(out var sp);

            int companyId;
            AppUser deleteUser;

            using (var scope = sp.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
                var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

                var newCompany = new Company
                {
                    Name = "DeleteMe Co",
                    NIP = "5555555555",
                    Address = "Delete Street 1",
                    Email = "deleteme@test.com",
                    Phone = "111222333"
                };
                db.Companies.Add(newCompany);
                await db.SaveChangesAsync();
                companyId = newCompany.Id;

                deleteUser = new AppUser
                {
                    UserName = "deleteuser",
                    Email = "deleteuser@test.com",
                    EmailConfirmed = true,
                    CompanyId = newCompany.Id,
                    CompanyMembershipStatus = CompanyMembershipStatus.Approved
                };

                var create = await um.CreateAsync(deleteUser, "Password123!");
                create.Succeeded.Should().BeTrue();

                var roleResult = await um.AddToRoleAsync(deleteUser, "Manager");
                roleResult.Succeeded.Should().BeTrue();

                await db.SaveChangesAsync();
            }

            var token = await IssueJwtForUserAsync(sp, deleteUser);
            SetBearer(client, token);

            var resp = await client.DeleteAsync("api/Company");
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);

            using (var scope2 = sp.CreateScope())
            {
                var db2 = scope2.ServiceProvider.GetRequiredService<StockWiseDb>();
                var deleted = await db2.Companies.FindAsync(companyId);
                deleted.Should().BeNull("firma przypisana do deleteuser powinna zostać usunięta bez konfliktów FK");
            }
        }
    }
}
