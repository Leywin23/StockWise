using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Dtos.OrderDtos;
using StockWise.Interfaces;
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
        private readonly IOrderService _orderService;
        public OrderController(StockWiseDb context, UserManager<AppUser> userManager, MoneyConverter moneyConverter, IOrderService orderService)
        {
            _context = context;
            _userManager = userManager;
            _moneyConverter = moneyConverter;
            _orderService = orderService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            var user = await _userManager.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.UserName == userName);

            var orders = await _orderService.GetOrdersAsync(user);

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

           var result = await _orderService.GetOrderAsync(user, id);
            
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> MakeOrder([FromBody] CreateOrderDto order)
        {
            var client = User.FindFirst(ClaimTypes.Name).Value;
            var user = await _userManager.Users.Include(u => u.Company).FirstOrDefaultAsync(u => u.UserName == client);

            var orderResult = await _orderService.MakeOrderAsync(order, user);
            return Ok(orderResult);
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

            var result = await _orderService.DeleteOrderAsync(user, id);

            return Ok(result);
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderDto dto)
        {
            var user = await GetCurrentUserAsync();
            
            var result = await _orderService.UpdateOrderAsync(id, dto, user, default);

            return Ok(result);
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
