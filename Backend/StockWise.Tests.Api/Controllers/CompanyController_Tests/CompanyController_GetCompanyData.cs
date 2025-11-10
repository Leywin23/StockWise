using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StockWise.Tests.Api.Controllers.CompanyController_Tests
{
    public class CompanyController_GetCompanyData :IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        public CompanyController_GetCompanyData(CustomWebAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetCompanyData_ShouldReturnOK()
        {
            var client = _factory.CreateClient();

            var resp = await client.GetAsync("api/Company/me");
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);
        }
    }
}
