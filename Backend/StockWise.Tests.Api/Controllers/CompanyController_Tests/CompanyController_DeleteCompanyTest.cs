using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        [Fact]
        public async Task DeleteCompanyTest_ShouldReturnOk()
        {
            var client = _factory.CreateClient();
            var resp = await client.DeleteAsync($"api/Company");

            var body = await resp.Content.ReadAsStringAsync();
            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);
        }
    }
}
