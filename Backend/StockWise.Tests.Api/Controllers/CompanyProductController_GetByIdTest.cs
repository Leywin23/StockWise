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
    public class CompanyProductController_GetByIdTest : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        public CompanyProductController_GetByIdTest(CustomWebAppFactory factory)
        {
            _factory = factory;
        }
        [Theory]
        [InlineData(1)]
        public async Task GetByIdCompanyProduct_ShouldReturnOK(int id)
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync($"api/CompanyProduct/{id}");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        [Theory]
        [InlineData(2)]
        public async Task GetByIdCompanyProduct_ShouldReturnNotFound(int id)
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync($"api/CompanyProduct/{id}");
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

    }
}
