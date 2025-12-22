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
    public class OrderController_UpdateOrderTest : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        public OrderController_UpdateOrderTest(CustomWebAppFactory factory)
        {
            _factory = factory;
        }
        [Fact]
        public async Task UpdateOrder_ShouldReturnOk_AndUpdateOrderLinesAndTotal()
        {
            int orderId;
            string eanOld = "11111111";
            string eanNew = "22222222";

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();

                var buyer = db.Companies.Single(c => c.NIP == "1234567890");

                var seller = db.Companies.FirstOrDefault(c => c.NIP != "1234567890");
                if (seller is null)
                {
                    seller = new Company
                    {
                        Name = "UpdateSeller",
                        NIP = "5555555555",
                        Address = "Seller St",
                        Email = "s@s.s",
                        Phone = "123123123"
                    };
                    db.Companies.Add(seller);
                    db.SaveChanges();
                }

                var category = db.Categories.FirstOrDefault() ?? new Category { Name = "UpdateCat" };
                if (category.CategoryId == 0)
                {
                    db.Categories.Add(category);
                    db.SaveChanges();
                }

                var productOld = new CompanyProduct
                {
                    CompanyProductName = "OldProduct",
                    EAN = eanOld,
                    Description = "Old",
                    Price = Money.Of(10m, "PLN"),
                    Stock = 100,
                    Company = seller,
                    Category = category,
                    IsAvailableForOrder = true
                };
                db.CompanyProducts.Add(productOld);

                var productNew = new CompanyProduct
                {
                    CompanyProductName = "NewProduct",
                    EAN = eanNew,
                    Description = "New",
                    Price = Money.Of(20m, "PLN"),
                    Stock = 100,
                    Company = seller,
                    Category = category,
                    IsAvailableForOrder = true
                };
                db.CompanyProducts.Add(productNew);
                db.SaveChanges();

                var order = new Order
                {
                    Seller = seller,
                    Buyer = buyer,
                    Status = OrderStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UserNameWhoMadeOrder = "loginuser",
                    ProductsWithQuantity = new List<OrderProduct>
            {
                new OrderProduct
                {
                    CompanyProductId = productOld.CompanyProductId,
                    Quantity = 2
                }
            },
                    TotalPrice = Money.Of(10m * 2, "PLN") 
                };

                db.Orders.Add(order);
                db.SaveChanges();

                orderId = order.Id;
            }

            var client = _factory.CreateClient();

            var updateOrderDto = new UpdateOrderDto
            {
                Currency = "EUR",
                ProductsEANWithQuantity = new Dictionary<string, int>
                {
                    [eanOld] = 0,
                    [eanNew] = 3
                }
            };

            var resp = await client.PutAsJsonAsync($"/api/Order/{orderId}", updateOrderDto);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.OK, body);

            using (var scope2 = _factory.Services.CreateScope())
            {
                var db2 = scope2.ServiceProvider.GetRequiredService<StockWiseDb>();

                var updated = db2.Orders
                    .Include(o => o.ProductsWithQuantity)
                        .ThenInclude(op => op.CompanyProduct)
                    .Single(o => o.Id == orderId);

                updated.ProductsWithQuantity
                    .Should().OnlyContain(op => op.CompanyProduct.EAN == eanNew);

                var newLine = updated.ProductsWithQuantity.Single();
                newLine.Quantity.Should().Be(3);
                newLine.CompanyProduct.EAN.Should().Be(eanNew);
                updated.TotalPrice.Currency.Code.Should().Be("EUR");

                updated.TotalPrice.Amount.Should().BeGreaterThan(0m);

            }
        }
        [Fact]
        public async Task UpdateOrder_WhenNegativeQuantity_ShouldReturn400()
        {
            var client = _factory.CreateClient();

            var dto = new UpdateOrderDto
            {
                Currency = "PLN",
                ProductsEANWithQuantity = new()
                {
                    ["12345678"] = -1
                }
            };

            var resp = await client.PutAsJsonAsync("/api/Order/1", dto);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
            body.Should().Contain("Quantity must be >= 0");
        }
        [Fact]
        public async Task UpdateOrder_WhenSomeEansMissingForSeller_ShouldReturn400_WithMissingEans()
        {
            int orderId;

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();

                var buyer = db.Companies.Single(c => c.NIP == "1234567890");
                var seller = db.Companies.First(c => c.NIP != "1234567890");

                var cat = db.Categories.FirstOrDefault() ?? new Category { Name = "UpdCatMissing" };
                if (cat.CategoryId == 0)
                {
                    db.Categories.Add(cat);
                    db.SaveChanges();
                }

                var prod = new CompanyProduct
                {
                    CompanyProductName = "Existing",
                    EAN = "33333333",
                    Description = "d",
                    Price = Money.Of(10, "PLN"),
                    Stock = 100,
                    Company = seller,
                    Category = cat,
                    IsAvailableForOrder = true
                };
                db.CompanyProducts.Add(prod);
                db.SaveChanges();

                var order = new Order
                {
                    Seller = seller,
                    Buyer = buyer,
                    Status = OrderStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UserNameWhoMadeOrder = "loginuser",
                    ProductsWithQuantity = new List<OrderProduct>(),
                    TotalPrice = Money.Of(12, "PLN")
                };
                db.Orders.Add(order);
                db.SaveChanges();
                orderId = order.Id;
            }

            var client = _factory.CreateClient();

            var dto = new UpdateOrderDto
            {
                Currency = "PLN",
                ProductsEANWithQuantity = new()
                {
                    ["11111111"] = 1,      
                    ["99999999"] = 2       
                }
            };

            var resp = await client.PutAsJsonAsync($"/api/Order/{orderId}", dto);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
            body.Should().Contain("Some products were not found for this seller");
        }
        [Fact]
        public async Task UpdateOrder_WhenStatusIsNotPending_ShouldReturn409()
        {
            int orderId;
            string ean = RandomEan8();

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();

                var buyer = db.Companies.Single(c => c.NIP == "1234567890");
                var seller = db.Companies.First(c => c.NIP != "1234567890");

                var cat = db.Categories.FirstOrDefault() ?? new Category { Name = "UpdCat" };
                if (cat.CategoryId == 0)
                {
                    db.Categories.Add(cat);
                    db.SaveChanges();
                }

                var prod = new CompanyProduct
                {
                    CompanyProductName = "P",
                    EAN = ean,
                    Description = "d",
                    Price = Money.Of(10, "PLN"),
                    Stock = 100,
                    CompanyId = seller.Id,     
                    CategoryId = cat.CategoryId,
                    IsAvailableForOrder = true
                };
                db.CompanyProducts.Add(prod);
                db.SaveChanges();

                var order = new Order
                {
                    SellerId = seller.Id,      
                    BuyerId = buyer.Id,
                    Status = OrderStatus.Accepted, 
                    CreatedAt = DateTime.UtcNow,
                    UserNameWhoMadeOrder = "loginuser",
                    TotalPrice = Money.Of(10, "PLN")
                };

                order.ProductsWithQuantity.Add(new OrderProduct
                {
                    CompanyProductId = prod.CompanyProductId,
                    Quantity = 1
                });

                db.Orders.Add(order);
                db.SaveChanges();

                orderId = order.Id;
            }

            var client = _factory.CreateClient();

            var dto = new UpdateOrderDto
            {
                Currency = "PLN",
                ProductsEANWithQuantity = new()
                {
                    [ean] = 5
                }
            };

            var resp = await client.PutAsJsonAsync($"/api/Order/{orderId}", dto);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.Conflict, body);
            body.Should().Contain("Only 'Pending' orders can be edited");
        }

        private static string RandomEan8()
        {

            var s = Guid.NewGuid().ToString("N");
            var digits = new string(s.Where(char.IsDigit).ToArray());
            if (digits.Length < 7) digits = digits.PadRight(7, '1');
            return "9" + digits.Substring(0, 7);
        }

        [Fact]
        public async Task UpdateOrder_WhenCompanyIsNotBuyer_ShouldReturn403()
        {
            int orderId;

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();

                var acme = db.Companies.Single(c => c.NIP == "1234567890");

                var seller = new Company { Name = "SellerX", NIP = "1111111111", Address = "x", Email = "s@s.s", Phone = "111111111" };
                var buyer = new Company { Name = "BuyerX", NIP = "2222222222", Address = "y", Email = "b@b.b", Phone = "222222222" };
                db.Companies.AddRange(seller, buyer);
                db.SaveChanges();

                var cat = new Category { Name = "UpdTest" };
                db.Categories.Add(cat);

                var prod = new CompanyProduct
                {
                    CompanyProductName = "P",
                    EAN = "11111111",
                    Description = "d",
                    Price = Money.Of(10, "PLN"),
                    Stock = 100,
                    Company = seller,
                    Category = cat,
                    IsAvailableForOrder = true
                };
                db.CompanyProducts.Add(prod);
                db.SaveChanges();

                var order = new Order
                {
                    Seller = seller,
                    Buyer = buyer, 
                    Status = OrderStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UserNameWhoMadeOrder = "xxx",
                    ProductsWithQuantity = new List<OrderProduct>
            {
                new OrderProduct { CompanyProductId = prod.CompanyProductId, Quantity = 1 }
            },
                    TotalPrice = Money.Of(10, "PLN")
                };
                db.Orders.Add(order);
                db.SaveChanges();

                orderId = order.Id;
            }

            var client = _factory.CreateClient();

            var dto = new UpdateOrderDto
            {
                Currency = "PLN",
                ProductsEANWithQuantity = new()
                {
                    ["11111111"] = 5
                }
            };

            var resp = await client.PutAsJsonAsync($"/api/Order/{orderId}", dto);
            var body = await resp.Content.ReadAsStringAsync();

            resp.StatusCode.Should().Be(HttpStatusCode.Forbidden, body);
        }
    }
}
