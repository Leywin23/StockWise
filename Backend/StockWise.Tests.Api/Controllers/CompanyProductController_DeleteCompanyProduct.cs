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
    public class CompanyProductController_DeleteCompanyProduct : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        public CompanyProductController_DeleteCompanyProduct(CustomWebAppFactory factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData(1)]
        public async Task DeleteCompanyProduct_ShouldReturnOk(int id)
        {
            var client = _factory.CreateClient();

            var resp = await client.DeleteAsync($"api/CompanyProduct/{id}");
            var body = await resp.Content.ReadAsStringAsync();
            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);
        }

        [Theory]
        [InlineData(999)]
        public async Task DeleteCompanyProduct_ShouldReturnNotFound(int id)
        {
            var client = _factory.CreateClient();

            var resp = await client.DeleteAsync($"api/CompanyProduct/{id}");
            var body = await resp.Content.ReadAsStringAsync();
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound, body);
        }
    }
}
