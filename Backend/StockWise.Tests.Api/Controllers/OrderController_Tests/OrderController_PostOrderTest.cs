using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StockWise.Application.Contracts.OrderDtos;
using StockWise.Infrastructure.Persistence;
using StockWise.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StockWise.Tests.Api.Controllers.OrderController_Tests
{
    public class OrderController_PostOrderTest : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        public OrderController_PostOrderTest(CustomWebAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task PostOrder_ShouldReturnOk_AndCreateOrder()
        {
            string otherNip = "1111111111";
            string otherEan = "87654321";
            decimal unitPrice = 7.50m;
            int qty = 3;

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
                var seller = new Company
                {
                    Name = "Other Seller",
                    NIP = otherNip,
                    Address = "Seller St 1",
                    Email = "seller@other.com",
                    Phone = "111111111"
                };
                db.Companies.Add(seller);

                var cat = db.Categories.FirstOrDefault() ?? new Category { Name = "X" };
                if (cat.CategoryId == 0) db.Categories.Add(cat);

                var prod = new CompanyProduct
                {
                    CompanyProductName = "Other Product",
                    EAN = otherEan,
                    Description = "Desc",
                    Price = Money.Of(unitPrice, "PLN"),
                    Stock = 500,
                    Company = seller,
                    Category = cat,
                    IsAvailableForOrder = true
                };
                db.CompanyProducts.Add(prod);

                db.SaveChanges();
            }
            var client = _factory.CreateClient();

            var dto = new CreateOrderDto
            {
                SellerName = "Other Seller",
                SellerNIP = otherNip,
                Address = "Seller St 1",
                Email = "seller@other.com",
                Phone = "111111111",
                Currency = "PLN",
                ProductsEANWithQuantity = new Dictionary<string, int>
                {
                    [otherEan] = qty
                }
            };

            var resp = await client.PostAsJsonAsync("/api/Order", dto);
            var body = await resp.Content.ReadAsStringAsync();
            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();   

                var order = db.Orders
                    .Include(o => o.Seller)
                    .Include(o => o.Buyer)
                    .Include(o => o.ProductsWithQuantity)
                    .ThenInclude(op => op.CompanyProduct)
                    .OrderByDescending(o => o.Id)
                    .FirstOrDefault(o => o.Seller.NIP == otherNip);

                order.Should().NotBeNull();
                order!.Seller.NIP.Should().Be(otherNip);
                order.Buyer.NIP.Should().Be("1234567890");

                order.ProductsWithQuantity.Should().HaveCount(1);
                var op = order.ProductsWithQuantity.Single();
                op.CompanyProduct.EAN.Should().Be(otherEan);
                op.Quantity.Should().Be(qty);

                order.TotalPrice.Currency.Code.Should().Be("PLN");
                order.TotalPrice.Amount.Should().Be(unitPrice * qty);
            }
        }
        [Fact]
        public async Task PostOrder_UnknownSellerNip_ShouldReturnNotFound()
        {
            var client = _factory.CreateClient();
            var dto = new CreateOrderDto
            {
                SellerName = "X",
                SellerNIP = "9999999999",
                Address = "A",
                Currency = "PLN",
                ProductsEANWithQuantity = new() { ["12345678"] = 1 }
            };
            var resp = await client.PostAsJsonAsync("/api/Order", dto);
            resp.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }
        [Fact]
        public async Task PostOrder_ProductEanNotFoundForSeller_ShouldReturnBadRequest()
        {
            var client = _factory.CreateClient();
            var dto = new CreateOrderDto
            {
                SellerName = "ACME",
                SellerNIP = "1234567890",
                Address = "123 Test Street",
                Currency = "PLN",
                ProductsEANWithQuantity = new() { ["00000000"] = 1 }
            };
            var resp = await client.PostAsJsonAsync("/api/Order", dto);
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task PostOrder_ZeroQuantity_ShouldReturnBadRequest()
        {
            var client = _factory.CreateClient();
            var dto = new CreateOrderDto
            {
                SellerName = "ACME",
                SellerNIP = "1234567890",
                Address = "123 Test Street",
                Currency = "PLN",
                ProductsEANWithQuantity = new() { ["12345678"] = 0 }
            };
            var resp = await client.PostAsJsonAsync("/api/Order", dto);
            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
