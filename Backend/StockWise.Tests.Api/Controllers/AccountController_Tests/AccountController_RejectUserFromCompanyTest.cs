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
    public class AccountController_RejectUserFromCompanyTest : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        public AccountController_RejectUserFromCompanyTest(CustomWebAppFactory factory)
        {
            _factory = factory;
        }

        private async Task<AppUser> CreatePendingUser(CustomWebAppFactory factory)
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
            var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            var company = db.Companies.First();
            var user = new AppUser
            {
                UserName = "Pending1",
                Email = "pending1@test.com",
                CompanyMembershipStatus = CompanyMembershipStatus.Pending,
                Company = company
            };

            var res = await um.CreateAsync(user, "Pas$word1");
            res.Succeeded.Should().BeTrue();

            return user;
        }
        private async Task<(AppUser manager, AppUser pendingUser)> CreateUsersForApproval(CustomWebAppFactory factory)
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
            var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            var newCompany = new Company
            {
                Name = "ApprovalCo",
                NIP = "8887776665",
                Address = "Approval City",
                Email = "manager@approval.com",
                Phone = "999888777"
            };
            db.Companies.Add(newCompany);
            await db.SaveChangesAsync();

            var manager = new AppUser
            {
                UserName = "ManagerUser",
                Email = "manager@approval.com",
                CompanyMembershipStatus = CompanyMembershipStatus.Approved,
                Company = newCompany
            };

            (await um.CreateAsync(manager, "Pas$word1")).Succeeded.Should().BeTrue();

            var pendingUser = new AppUser
            {
                UserName = "PendingUser",
                Email = "pending@approval.com",
                Company = newCompany,
                CompanyMembershipStatus = CompanyMembershipStatus.Pending
            };

            (await um.CreateAsync(pendingUser, "Pas$word1")).Succeeded.Should().BeTrue();

            return (manager, pendingUser);
        }

        private async Task<AppUser> CreatePendingUserOtherCompany(CustomWebAppFactory factory)
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
            var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            var company = new Company
            {
                Name = "Other Co",
                NIP = "9998887771",
                Address = "Testx",
                Email = "o@o.o",
                Phone = "123123123"
            };

            db.Companies.Add(company);
            db.SaveChanges();

            var user = new AppUser
            {
                UserName = "Pending2",
                Email = "pending2@test.com",
                CompanyMembershipStatus = CompanyMembershipStatus.Pending,
                Company = company
            };

            var res = await um.CreateAsync(user, "Pas$word1");
            res.Succeeded.Should().BeTrue();

            return user;
        }

        [Fact]
        public async Task ApproveUser_ShouldReturnOk_AndApproveUser()
        {
            var user = await CreatePendingUser(_factory);

            var client = _factory.CreateClient();
            var resp = await client.PostAsync($"/api/Account/approve-user/{user.Id}", null);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);

            using var scope = _factory.Services.CreateScope();
            var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var reloaded = await um.FindByIdAsync(user.Id);

            reloaded!.CompanyMembershipStatus.Should().Be(CompanyMembershipStatus.Approved);
        }


        [Fact]
        public async Task ApproveUser_ShouldReturnBadRequest_WhenUserFromAnotherCompany()
        {
            var user = await CreatePendingUserOtherCompany(_factory);

            var client = _factory.CreateClient();
            var resp = await client.PostAsync($"/api/Account/approve-user/{user.Id}", null);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
            body.Should().Contain("User does not belong to your company.");

            using var scope = _factory.Services.CreateScope();
            var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var reloaded = await um.FindByIdAsync(user.Id);

            reloaded!.CompanyMembershipStatus.Should().Be(CompanyMembershipStatus.Pending);
        }


        [Fact]
        public async Task ApproveUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            var client = _factory.CreateClient();

            var resp = await client.PostAsync("/api/Account/approve-user/x123-non-exist", null);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.NotFound, body);
            body.Should().Contain("User not found");
        }


        [Fact]
        public async Task ApproveUser_ShouldReturnBadRequest_WhenUserNotPending()
        {
            AppUser user;

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
                var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

                var company = db.Companies.First();

                user = new AppUser
                {
                    UserName = "ApprovedBefore",
                    Email = "approved@test.com",
                    Company = company,
                    CompanyMembershipStatus = CompanyMembershipStatus.Approved
                };

                (await um.CreateAsync(user, "Pas$word1")).Succeeded.Should().BeTrue();
            }

            var client = _factory.CreateClient();
            var resp = await client.PostAsync($"/api/Account/approve-user/{user.Id}", null);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
            body.Should().Contain("User is not in Pending status.");
        }

    }
}
