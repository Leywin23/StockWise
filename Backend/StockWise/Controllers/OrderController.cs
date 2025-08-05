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
            var userName = User.FindFirst(ClaimTypes.Name).Value;
            var user = await _userManager.Users.Include(u=>u.Company).FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var company = await _context.Companies.FirstOrDefaultAsync(c => c.NIP == user.Company.NIP);
            if (company == null)
            {
                return BadRequest("User does not belong to any company.");
            }

            var orders = await _context.Orders.Where(o => o.BuyerId == company.Id|| o.SellerId == company.Id).ToListAsync();

            return Ok(orders);
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> MakeOrder([FromBody] CreateOrderDto order)
        {
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

            List<CompanyProduct> orderedProducts = await _context.CompanyProducts.Where(p=>order.ProductsEAN.Contains(p.EAN)).ToListAsync();
            if (orderedProducts.Count != order.ProductsEAN.Count)
            {
                return BadRequest("Some products were not found.");
            }

            var unavailable = orderedProducts.Where(p=>!p.IsAvailableForOrder).ToList();

            if (unavailable.Any()) {
                var unavailableDetails = unavailable.Select(p=> $"Name: `{p.CompanyProductName}`, EAN: {p.EAN}").ToList();

                return BadRequest(new
                {
                    Message = "Some products are not available for order.",
                    Products = unavailable.Select(p => new {
                        p.CompanyProductName,
                        p.EAN,
                        p.Description
                    })
                });
            }


            var newOrder = new Order
            {
                SellerId = seller.Id,
                Seller = seller,
                BuyerId = buyer.Id,
                Buyer = buyer,
                CreatedAt = DateTime.Now,
                Products = orderedProducts
            };

            await _context.Orders.AddAsync(newOrder);
            await _context.SaveChangesAsync();

            return Ok(order);
        }
    }
}
