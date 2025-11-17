using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StockWise.Tests.Api.Controllers.EmailController_Tests
{
    public class EmailController_SendEmailTest : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        public EmailController_SendEmailTest(CustomWebAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task SendEmail_ShouldReturnOk()
        {
            var client = _factory.CreateClient();
 

            var resp = await client.PostAsync(
        "api/Email?email=test@test.com&subject=Hello&body=TestBody",
        content: null);

            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);
        }
    }
}
