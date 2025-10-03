using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Dtos.OrderDtos;
using StockWise.Interfaces;
using StockWise.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http.HttpResults;
using StockWise.Helpers;

namespace StockWise.Services
{
    public class OrderService : IOrderService
    {
        private readonly StockWiseDb _context;
        private readonly MoneyConverter _moneyConverter;

        public OrderService(StockWiseDb context, MoneyConverter moneyConverter)
        {
            _context = context;
            _moneyConverter = moneyConverter;
        }

        public async Task<ServiceResult<OrderListDto>> MakeOrderAsync(CreateOrderDto order, AppUser user)
        {
            foreach (var kvp in order.ProductsEANWithQuantity)
            {
                if (kvp.Value <= 0)
                {
                    return ServiceResult<OrderListDto>.BadRequest("Quantity must be greater than 0");
                }
            }

            if (user == null) 
            {
                return ServiceResult<OrderListDto>.NotFound("User not found");
            }

            if(user.Company == null)
            {
                return ServiceResult<OrderListDto>.NotFound("User does not belong to any company");
            }

           var buyer = await _context.Companies.FirstOrDefaultAsync(c => c.Id == user.CompanyId);
           var seller = await _context.Companies.FirstOrDefaultAsync(c=>c.NIP== order.SellerNIP);

           if(seller == null)
           {
                return ServiceResult<OrderListDto>.NotFound("Seller not found.");
           }

            Dictionary<CompanyProduct, int> orderedProducts = await _context.CompanyProducts.Where(p => order.ProductsEANWithQuantity.Keys.Contains(p.EAN)).ToDictionaryAsync(
                  p => p,
                  p => order.ProductsEANWithQuantity[p.EAN]
                  );

            if (orderedProducts.Count != order.ProductsEANWithQuantity.Count) {
               var found = orderedProducts.Keys.Select(p=>p.EAN).ToHashSet(StringComparer.OrdinalIgnoreCase);
               var missing = order.ProductsEANWithQuantity.Where(p => !found.Contains(p.Key)).ToArray();

               return ServiceResult<OrderListDto>.NotFound("Some products were not found.") with { Details = new { MissingEANs = missing } };
           };

            var unavailable = orderedProducts.Where(p => !p.Key.IsAvailableForOrder).ToList();

            if (unavailable.Any())
            {
                var unavailableDetails = unavailable.Select(p => $"Name: `{p.Key.CompanyProductName}`, EAN: {p.Key.EAN}").ToList();

                return ServiceResult<OrderListDto>.BadRequest(
                    "Some products are not available for order.",
                    new
                    {
                        Products = unavailable.Select(p => new
                        {
                            p.Key.CompanyProductName,
                            p.Key.EAN,
                            p.Key.Description
                        })
                    }
                );
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
                TotalPrice = totalPrice,
                Status = OrderStatus.Pending
            };

            await _context.Orders.AddAsync(newOrder);
            await _context.SaveChangesAsync();

            var result = new OrderListDto
            {
                Status = newOrder.Status,
                CreatedAt = newOrder.CreatedAt,
                Seller = new CompanyMiniDto
                {
                    Name = newOrder.Seller.Name,
                    NIP = newOrder.Seller.NIP,
                },
                Buyer = new CompanyMiniDto
                {
                    Name = newOrder.Buyer.Name,
                    NIP = newOrder.Buyer.NIP,
                },
                UserNameWhoMadeOrder = newOrder.UserNameWhoMadeOrder,
                TotalPrice = totalPrice,
                ProductsWithQuantity = newOrder.ProductsWithQuantity
                .Select(p =>
                {
                    return new ProductWithQuantityDto
                    {
                        Product = new CompanyProductMiniDto
                        {
                            Id = p.CompanyProductId,
                            CompanyProductName = p.CompanyProduct.CompanyProductName,
                            EAN = p.CompanyProduct.EAN,
                            Price = p.CompanyProduct.Price
                        },
                        Quantity = p.Quantity
                    };
                })
                .ToList()
                };

            return ServiceResult<OrderListDto>.Ok(result);
        }

        public async Task<ServiceResult<OrderListDto>> UpdateOrderAsync(int id, UpdateOrderDto dto, AppUser user, CancellationToken ct = default)
        {
            if (dto is null) return ServiceResult<OrderListDto>.BadRequest("Body is required.");
            if (dto.ProductsEANWithQuantity is null) return ServiceResult<OrderListDto>.BadRequest("ProductsEANWithQuantity is required.");
            if (dto.ProductsEANWithQuantity.Count == 0) return ServiceResult<OrderListDto>.BadRequest("At least one product is required.");
            if (dto.ProductsEANWithQuantity.Any(kvp => kvp.Value < 0))
                return ServiceResult<OrderListDto>.BadRequest("Quantity must be >= 0 (use 0 to remove a line).");

            if (user is null) return ServiceResult<OrderListDto>.Unauthorized("User not found.");
            if (user.Company is null) return ServiceResult<OrderListDto>.BadRequest("User does not belong to any company.");

            var order = await _context.Orders
                .Include(o => o.Buyer)
                .Include(o => o.Seller)
                .Include(o => o.ProductsWithQuantity)
                    .ThenInclude(op => op.CompanyProduct)
                .FirstOrDefaultAsync(o => o.Id == id, ct);

            if (order is null) return ServiceResult<OrderListDto>.NotFound($"Order with id: {id} not found.");

            if (order.Buyer?.Id != user.Company.Id)
                return ServiceResult<OrderListDto>.Forbidden("Only the buyer's company can edit this order.");

            if (order.Status != OrderStatus.Pending)
                return ServiceResult<OrderListDto>.Conflict($"Order status is {order.Status}. Only 'Pending' orders can be edited.");

            var requestedEANs = dto.ProductsEANWithQuantity.Keys
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var sellerProducts = await _context.CompanyProducts
                .Where(cp => cp.CompanyId == order.SellerId && requestedEANs.Contains(cp.EAN))
                .ToListAsync(ct);

            if (sellerProducts.Count != requestedEANs.Count)
            {
                var found = sellerProducts.Select(p => p.EAN).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var missing = requestedEANs.Where(e => !found.Contains(e)).ToArray();
                return ServiceResult<OrderListDto>.BadRequest("Some products were not found for this seller.")
                    with
                { Details = new { MissingEANs = missing } };
            }

            var unavailable = sellerProducts.Where(sp => !sp.IsAvailableForOrder).ToList();
            if (unavailable.Any())
            {
                return ServiceResult<OrderListDto>.BadRequest("Some products are not available for order.")
                    with
                { Details = unavailable.Select(p => new { p.CompanyProductName, p.EAN, p.Description }) };
            }

            var productsByEAN = sellerProducts.ToDictionary(p => p.EAN, p => p, StringComparer.OrdinalIgnoreCase);

            await using var tx = await _context.Database.BeginTransactionAsync(ct);
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

                var ids = order.ProductsWithQuantity.Select(x => x.CompanyProductId).ToHashSet();
                var priceMap = await _context.CompanyProducts
                    .AsNoTracking()
                    .Where(p => ids.Contains(p.CompanyProductId))
                    .Select(p => new { p.CompanyProductId, p.Price })
                    .ToDictionaryAsync(x => x.CompanyProductId, x => x.Price, ct);

                decimal total = 0m;
                foreach (var line in order.ProductsWithQuantity)
                {
                    var converted = await _moneyConverter.ConvertAsync(
                        priceMap[line.CompanyProductId], targetCurrency);
                    total += converted.Amount * line.Quantity;
                }
                order.TotalPrice = Money.Of(total, targetCurrency);

                await _context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

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

                return ServiceResult<OrderListDto>.Ok(result);
            }
            catch (DbUpdateConcurrencyException)
            {
                await tx.RollbackAsync(ct);
                return ServiceResult<OrderListDto>.Conflict("The order was modified by another process. Reload and retry.");
            }
            catch (OperationCanceledException)
            {
                await tx.RollbackAsync(ct);
                return ServiceResult<OrderListDto>.ServerError("Client Closed Request");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                return ServiceResult<OrderListDto>.ServerError($"Server error: {ex.Message}");
            }
        }
    }
}
