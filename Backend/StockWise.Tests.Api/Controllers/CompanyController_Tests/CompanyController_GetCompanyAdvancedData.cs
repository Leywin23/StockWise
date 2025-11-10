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
    
    public class CompanyController_GetCompanyAdvancedData:IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        public CompanyController_GetCompanyAdvancedData(CustomWebAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetAdvancedCompanyData()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("api/Company/me/advanced");
            var body = await resp.Content.ReadAsStringAsync();
            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);
        }
    }
}
