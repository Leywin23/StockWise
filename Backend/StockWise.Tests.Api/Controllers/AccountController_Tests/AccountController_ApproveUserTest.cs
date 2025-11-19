using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.Utilities.Net;
using StockWise.Infrastructure.Persistence;
using StockWise.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StockWise.Tests.Api.Controllers.AccountController_Tests
{
    public class AccountController_ApproveUserTest : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        public AccountController_ApproveUserTest(CustomWebAppFactory factory)
        {
            _factory = factory;
        }
        private async Task<AppUser> CreatePendingUser(CustomWebAppFactory factory)
        {
            var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
            var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var company = db.Companies.First();
            var user = new AppUser
            {
                UserName = "Test1",
                Email = "Test1@test.com",
                CompanyMembershipStatus = CompanyMembershipStatus.Pending,
                Company = company,
            };
            var res = await um.CreateAsync(user, "Pas$word1");
            res.Succeeded.Should().BeTrue();

            return user;
        }

        private async Task<AppUser> CreateApprovedUser(CustomWebAppFactory factory)
        {
            var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
            var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var company = db.Companies.First();
            var user = new AppUser
            {
                UserName = "Test1",
                Email = "Test1@test.com",
                CompanyMembershipStatus = CompanyMembershipStatus.Approved,
                Company = company,
            };

            var res = await um.CreateAsync(user, "Pas$word1");
            res.Succeeded.Should().BeTrue();

            return user;
        }
        private async Task<AppUser> CreatePendingUserAndComapny(CustomWebAppFactory factory)
        {
            var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
            var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var newCompany = new Company
            {
                Name = "OtherComapny",
                NIP = "1234567899",
                Address = "Test city",
                Email = "OtherComapny@test.com",
                Phone = "123456788"
            };
            var comresp = await db.Companies.AddAsync(newCompany);
            await db.SaveChangesAsync();

            var user = new AppUser
            {
                UserName = "Test1",
                Email = "Test1@test.com",
                CompanyMembershipStatus = CompanyMembershipStatus.Pending,
                Company = newCompany,
            };
            var res = await um.CreateAsync(user, "Pas$word1");
            res.Succeeded.Should().BeTrue();

            return user;
        }

        [Fact]
        public async Task ApproveUserTest_ShouldReturnOkAndApproveUser()
        {
            var user = await CreatePendingUser(_factory);
            var client = _factory.CreateClient();
            var resp = await client.PostAsync($"api/Account/approve-user/{user.Id}", null);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);

            using (var scope = _factory.Services.CreateScope())
            {
                var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                var approvedUser = await um.FindByIdAsync(user.Id);
                approvedUser.Should().NotBeNull();

                approvedUser!.CompanyMembershipStatus.Should().Be(CompanyMembershipStatus.Approved);
            }

        }

        [Fact]
        public async Task ApproveUserTest_ShouldReturnBadRequestUserFromAnotherCompany()
        {
            var user = await CreatePendingUserAndComapny(_factory);
            var client = _factory.CreateClient();
            var resp = await client.PostAsync($"api/Account/approve-user/{user.Id}", null);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);

            using (var scope = _factory.Services.CreateScope())
            {
                var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                var approvedUser = await um.FindByIdAsync(user.Id);
                approvedUser.Should().NotBeNull();

                approvedUser!.CompanyMembershipStatus.Should().Be(CompanyMembershipStatus.Pending);
            }
            body.Should().NotBeNull();
            body.Should().Contain("User does not belong to your company.");
        }

        [Fact]
        public async Task ApproveUser_ShouldReturnBadRequest_WhenUserIsNotPending()
        {
            var user = await CreateApprovedUser(_factory);

            var client = _factory.CreateClient();
            var resp = await client.PostAsync($"/api/Account/approve-user/{user.Id}", null);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
            body.Should().Contain("User is not in Pending status.");

            using (var scope = _factory.Services.CreateScope())
            {
                var um2 = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                var reloaded = await um2.FindByIdAsync(user.Id);
                reloaded!.CompanyMembershipStatus.Should().Be(CompanyMembershipStatus.Approved);
            }
        }

        [Fact]
        public async Task ApproveUser_ShouldReturnBadRequest_WhenCurrentUserHasNoCompany()
        {
            AppUser userToApprove;

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
                var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

                var currentUser = await um.FindByIdAsync("u1");
                currentUser.Should().NotBeNull();

                currentUser!.CompanyId = null;
                currentUser.Company = null;

                var updateRes = await um.UpdateAsync(currentUser);
                updateRes.Succeeded.Should().BeTrue();

                var company = db.Companies.First();
                userToApprove = new AppUser
                {
                    UserName = "PendingUserNoManagerCompany",
                    Email = "pending-nomanager@test.com",
                    CompanyMembershipStatus = CompanyMembershipStatus.Pending,
                    Company = company
                };

                var res = await um.CreateAsync(userToApprove, "Pas$word1");
                res.Succeeded.Should().BeTrue();
            }

            var client = _factory.CreateClient();

            var resp = await client.PostAsync($"/api/Account/approve-user/{userToApprove.Id}", null);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
            body.Should().Contain("You are not assigned to a company.");

            using (var scope2 = _factory.Services.CreateScope())
            {
                var um2 = scope2.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                var reloaded = await um2.FindByIdAsync(userToApprove.Id);
                reloaded.Should().NotBeNull();
                reloaded!.CompanyMembershipStatus.Should().Be(CompanyMembershipStatus.Pending);
            }
        }
    }
}
