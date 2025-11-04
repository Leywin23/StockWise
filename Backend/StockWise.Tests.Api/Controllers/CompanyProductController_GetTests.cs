using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StockWise.Tests.Api.Controllers
{
    public class CompanyProductController_GetTests : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        public CompanyProductController_GetTests(CustomWebAppFactory factory) => _factory = factory;

        [Fact]
        public async Task GetCompanyProducts_ShouldReturnOk()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("/api/CompanyProduct?page=1&pageSize=10");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
