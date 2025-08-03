using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Dtos;
using StockWise.Models;

namespace StockWise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly StockWiseDb _context;
        public OrderController(StockWiseDb context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> MakeOrder(CreateOrderDto order)
        {
            var client = User.Identity.Name;
            var seller = _context.Companies.FirstOrDefault(c=>c.NIP == order.SellerNIP);
            if (client == null || seller == null)
            {
                return NotFound("Buyer or Seller not found.");
            }

            List<Product> orderedProducts = await _context.Products.Where(p=>order.ProductsEAN.Contains(p.EAN)).ToListAsync();
            if (orderedProducts.Count != order.ProductsEAN.Count)
            {
                return BadRequest("Some products were not found.");
            }

            var newOrder = new Order
            {
                SellerId = seller.Id,
                Seller = seller,
                //BuyerId = client.Id,
                //Buyer = client,

                Status = order.Status,
                Products = orderedProducts
            };

            await _context.Orders.AddAsync(newOrder);
            await _context.SaveChangesAsync();

            return Ok(order);
        }
    }
}
