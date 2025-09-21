using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Dtos.OrderDtos;
using StockWise.Models;
using StockWise.Services;
using System.Security.Claims;

namespace StockWise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly StockWiseDb _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly MoneyConverter _moneyConverter;
        public OrderController(StockWiseDb context, UserManager<AppUser> userManager, MoneyConverter moneyConverter)
        {
            _context = context;
            _userManager = userManager;
            _moneyConverter = moneyConverter;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            var user = await _userManager.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.UserName == userName);

            if (user == null) return NotFound("User not found");
            if (user.Company == null) return BadRequest("User does not belong to any company.");

            var companyNip = user.Company.NIP;

            var orders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.Seller.NIP == companyNip || o.Buyer.NIP == companyNip)
                .Select(o => new OrderListDto
                {
                    Id = o.Id,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt,
                    UserNameWhoMadeOrder = o.UserNameWhoMadeOrder,
                    TotalPrice = o.TotalPrice,
                    Seller = new CompanyMiniDto
                    {
                        Id = o.SellerId,
                        Name = o.Seller.Name,
                        NIP = o.Seller.NIP
                    },
                    Buyer = new CompanyMiniDto
                    {
                        Id = o.BuyerId,
                        Name = o.Buyer.Name,
                        NIP = o.Buyer.NIP
                    },
                    ProductsWithQuantity = o.ProductsWithQuantity
                        .Select(op => new ProductWithQuantityDto
                        {
                            Product = new CompanyProductMiniDto
                            {
                                Id = op.CompanyProduct.CompanyProductId,
                                CompanyProductName = op.CompanyProduct.CompanyProductName,
                                EAN = op.CompanyProduct.EAN,
                                Price = op.CompanyProduct.Price 
                            },
                            Quantity = op.Quantity
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) {
                return Unauthorized("User not found");
            }

            var order = await _context.Orders
                .Include(o=>o.Seller)
                .Include(o=>o.Buyer)
                .Include(o=>o.ProductsWithQuantity)
                .ThenInclude(op=>op.CompanyProduct)
                .FirstOrDefaultAsync(o=>o.Id == id);

            if(order == null)
            {
                return NotFound("Product not found");
            }

            var result = new OrderListDto
            {
                Id = order.Id,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                UserNameWhoMadeOrder = order.UserNameWhoMadeOrder,
                TotalPrice = order.TotalPrice,
                Seller = new CompanyMiniDto
                {
                    Id = order.Seller.Id,
                    Name = order.Seller.Name,
                    NIP = order.Seller.NIP
                },
                Buyer = new CompanyMiniDto
                {
                    Id = order.Buyer.Id,
                    Name = order.Buyer.Name,
                    NIP = order.Buyer.NIP
                },
                ProductsWithQuantity = order.ProductsWithQuantity.Select(p => new ProductWithQuantityDto
                {
                    Product = new CompanyProductMiniDto
                    {
                        Id = p.CompanyProductId,
                        CompanyProductName = p.CompanyProduct.CompanyProductName,
                        EAN = p.CompanyProduct.EAN,
                        Price = p.CompanyProduct.Price,
                    },
                    Quantity = p.Quantity,
                }).ToList(),
            };
            
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> MakeOrder([FromBody] CreateOrderDto order)
        {
            foreach (var kvp in order.ProductsEANWithQuantity) {
                if(kvp.Value < 0)
                {
                    return BadRequest("Quantity must be greater than 0");
                }
            }

            var client = User.FindFirst(ClaimTypes.Name).Value;
            var user = await _userManager.Users.Include(u => u.Company).FirstOrDefaultAsync(u => u.UserName == client);


            if (user == null) {
                return NotFound("User not found");
            }

            if (user.Company == null)
            {
                return BadRequest("User does not belong to any company.");
            }

            var buyer = await _context.Companies.FirstOrDefaultAsync(c=>c.NIP== user.Company.NIP);
            

            var seller = await _context.Companies.FirstOrDefaultAsync(c=>c.NIP == order.SellerNIP);

            if (seller == null)
            {
                return NotFound("Seller not found.");
            }

            Dictionary<CompanyProduct, int> orderedProducts = await _context.CompanyProducts.Where(p => order.ProductsEANWithQuantity.Keys.Contains(p.EAN)).ToDictionaryAsync(
                p=>p,
                p => order.ProductsEANWithQuantity[p.EAN]
                );

            if (orderedProducts.Count != order.ProductsEANWithQuantity.Count)
            {
                return BadRequest("Some products were not found.");
            }

            var unavailable = orderedProducts.Where(p=>!p.Key.IsAvailableForOrder).ToList();

            if (unavailable.Any()) {
                var unavailableDetails = unavailable.Select(p=> $"Name: `{p.Key.CompanyProductName}`, EAN: {p.Key.EAN}").ToList();

                return BadRequest(new
                {
                    Message = "Some products are not available for order.",
                    Products = unavailable.Select(p => new {
                        p.Key.CompanyProductName,
                        p.Key.EAN,
                        p.Key.Description
                    })
                });
            }


            var productsWithQuantity = orderedProducts
            .Select(kvp => new OrderProduct
            {
                CompanyProductId = kvp.Key.CompanyProductId,
                Quantity = kvp.Value
            })
            .ToList();

            var total = 0.0m;
            foreach (var kvp in orderedProducts)
            {
                var price = await _moneyConverter.ConvertAsync(kvp.Key.Price, order.Currency);
                total += price.Amount * kvp.Value;
            }

            var totalPrice = Money.Of(total, order.Currency);

            var newOrder = new Order
            {
                SellerId = seller.Id,
                Seller = seller,
                BuyerId = buyer.Id,
                Buyer = buyer,
                CreatedAt = DateTime.Now,
                ProductsWithQuantity = productsWithQuantity,
                UserNameWhoMadeOrder = user.UserName,
                TotalPrice = totalPrice
            };

            await _context.Orders.AddAsync(newOrder);
            await _context.SaveChangesAsync();

            return Ok(order);
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteOrderAsync(int id)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return NotFound("User not found");
            }

            var comapany = user.Company;
            if(comapany == null)
            {
                return BadRequest("User does not belong to any company.");
            }

            var order = await _context.Orders
                .Include(o=>o.Buyer)
                .Include(o=>o.Seller)
                .Include(o=>o.ProductsWithQuantity)
                .FirstOrDefaultAsync(o=>o.Id==id);

            if (order == null) {
                return NotFound($"Order with id: {id} not found");
            }

            _context.Orders.Remove(order);
            _context.SaveChanges();

            var result = new OrderListDto
            {
                Id = order.Id,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                UserNameWhoMadeOrder = order.UserNameWhoMadeOrder,
                TotalPrice = order.TotalPrice,
                Buyer = new CompanyMiniDto
                {
                    Id = order.Buyer.Id,
                    Name = order.Buyer.Name,
                    NIP = order.Buyer.NIP
                },
                Seller = new CompanyMiniDto
                {
                    Id = order.Seller.Id,
                    Name = order.Seller.Name,
                    NIP = order.Seller.NIP
                },
                ProductsWithQuantity = order.ProductsWithQuantity.Select(p => new ProductWithQuantityDto
                {
                    Product = new CompanyProductMiniDto
                    {
                        Id = p.CompanyProductId,
                        CompanyProductName = p.CompanyProduct.CompanyProductName,
                        EAN = p.CompanyProduct.EAN,
                        Price = p.CompanyProduct.Price,
                    },
                    Quantity = p.Quantity,
                }).ToList(),
            };

            return Ok(result);
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderDto dto)
        {
            if (dto is null) return BadRequest("Body is required.");
            if (dto.ProductsEANWithQuantity is null) return BadRequest("ProductsEANWithQuantity is required.");
            if (dto.ProductsEANWithQuantity.Count == 0) return BadRequest("At least one product is required.");
            if (dto.ProductsEANWithQuantity.Any(kvp => kvp.Value < 0))
                return BadRequest("Quantity must be >= 0 (use 0 to remove a line).");

            var user = await GetCurrentUserAsync();
            if (user == null) return NotFound("User not found.");
            var company = user.Company;
            if (company == null) return BadRequest("User does not belong to any company.");

            var order = await _context.Orders
                .Include(o => o.Buyer)
                .Include(o => o.Seller)
                .Include(o => o.ProductsWithQuantity)
                    .ThenInclude(op => op.CompanyProduct)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound($"Order with id: {id} not found.");
            if (order.Buyer?.Id != company.Id) return Forbid("Only the buyer's company can edit this order.");
            if (!string.Equals(order.Status, "Pending", StringComparison.OrdinalIgnoreCase))
                return Conflict($"Order status is {order.Status}. Only 'Pending' orders can be edited");

            var requestedEANs = dto.ProductsEANWithQuantity.Keys
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var sellerProducts = await _context.CompanyProducts
                .Where(cp => cp.CompanyId == order.SellerId && requestedEANs.Contains(cp.EAN))
                .ToListAsync();

            if (sellerProducts.Count != requestedEANs.Count)
            {
                var found = sellerProducts.Select(p => p.EAN).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var missing = requestedEANs.Where(e => !found.Contains(e)).ToArray();
                return BadRequest(new
                {
                    Message = "Some products were not found for this seller.",
                    MissingEANs = missing
                });
            }

            var unavailable = sellerProducts.Where(sp => !sp.IsAvailableForOrder).ToList();
            if (unavailable.Any())
            {
                return BadRequest(new
                {
                    Message = "Some products are not available for order.",
                    Products = unavailable.Select(p => new { p.CompanyProductName, p.EAN, p.Description }).ToList()
                });
            }

            var productsByEAN = sellerProducts.ToDictionary(p => p.EAN, p => p, StringComparer.OrdinalIgnoreCase);

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var currentByProductId = order.ProductsWithQuantity
                    .ToDictionary(op => op.CompanyProductId);

                foreach (var (ean, qty) in dto.ProductsEANWithQuantity)
                {
                    var cp = productsByEAN[ean];

                    if (qty == 0)
                    {
                        if (currentByProductId.TryGetValue(cp.CompanyProductId, out var toRemove))
                        {
                            _context.Remove(toRemove);
                            order.ProductsWithQuantity.Remove(toRemove);
                            currentByProductId.Remove(cp.CompanyProductId);
                        }
                        continue;
                    }

                    if (currentByProductId.TryGetValue(cp.CompanyProductId, out var existing))
                    {
                        existing.Quantity = qty;
                        existing.CompanyProductId = cp.CompanyProductId;
                    }
                    else
                    {
                        var added = new OrderProduct
                        {
                            CompanyProductId = cp.CompanyProductId,
                            Quantity = qty
                        };
                        order.ProductsWithQuantity.Add(added);
                        currentByProductId[cp.CompanyProductId] = added;
                    }
                }

                var targetCurrency = string.IsNullOrWhiteSpace(dto.Currency)
                    ? (order.TotalPrice != null ? order.TotalPrice.Currency.ToString() : "PLN")
                    : dto.Currency;

                var ids = order.ProductsWithQuantity
                    .Select(x => x.CompanyProductId)
                    .ToHashSet();

                var priceMap = await _context.CompanyProducts
                    .AsNoTracking() 
                    .Where(p => ids.Contains(p.CompanyProductId))
                    .Select(p => new { p.CompanyProductId, p.Price })
                    .ToDictionaryAsync(x => x.CompanyProductId, x => x.Price);

                decimal total = 0m;
                foreach (var line in order.ProductsWithQuantity)
                {
                    var converted = await _moneyConverter.ConvertAsync(
                        priceMap[line.CompanyProductId], targetCurrency);
                    total += converted.Amount * line.Quantity;
                }
                order.TotalPrice = Money.Of(total, targetCurrency);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                var result = new OrderListDto
                {
                    Id = order.Id,
                    Status = order.Status,
                    CreatedAt = order.CreatedAt,
                    UserNameWhoMadeOrder = order.UserNameWhoMadeOrder,
                    TotalPrice = order.TotalPrice,
                    Seller = new CompanyMiniDto { Id = order.Seller.Id, Name = order.Seller.Name, NIP = order.Seller.NIP },
                    Buyer = new CompanyMiniDto { Id = order.Buyer.Id, Name = order.Buyer.Name, NIP = order.Buyer.NIP },
                    ProductsWithQuantity = order.ProductsWithQuantity.Select(p =>
                    {
                        var cp = sellerProducts.First(sp => sp.CompanyProductId == p.CompanyProductId);
                        return new ProductWithQuantityDto
                        {
                            Product = new CompanyProductMiniDto
                            {
                                Id = cp.CompanyProductId,
                                CompanyProductName = cp.CompanyProductName,
                                EAN = cp.EAN,
                                Price = cp.Price
                            },
                            Quantity = p.Quantity
                        };
                    }).ToList()
                };

                return Ok(result);
            }
            catch (DbUpdateConcurrencyException)
            {
                await tx.RollbackAsync();
                return Conflict("The order was modified by another process. Reload and retry");
            }
            catch (OperationCanceledException)
            {
                await tx.RollbackAsync();
                return StatusCode(499, "Client Closed Request");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        private async Task<AppUser?> GetCurrentUserAsync()
        {
            var userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName)) return null;

            return await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.UserName == userName);
        }
    }
}
