using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Dtos.OrderDtos;
using StockWise.Models;
using System.Security.Claims;

namespace StockWise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly StockWiseDb _context;
        private readonly UserManager<AppUser> _userManager;
        public OrderController(StockWiseDb context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

            var newOrder = new Order
            {
                SellerId = seller.Id,
                Seller = seller,
                BuyerId = buyer.Id,
                Buyer = buyer,
                CreatedAt = DateTime.Now,
                ProductsWithQuantity = productsWithQuantity,
                UserNameWhoMadeOrder = user.UserName
            };

            await _context.Orders.AddAsync(newOrder);
            await _context.SaveChangesAsync();

            return Ok(order);
        }
    }
}
