using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StockWise.Application.Contracts.OrderDtos;
using StockWise.Application.Interfaces;
using StockWise.Extensions;
using StockWise.Infrastructure.Persistence;
using StockWise.Infrastructure.Services;
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
        private readonly MoneyConverter _moneyConverter;
        private readonly IOrderService _orderService;
        private readonly ICurrentUserService _currentUserService;
        public OrderController(StockWiseDb context, UserManager<AppUser> userManager, MoneyConverter moneyConverter, IOrderService orderService, ICurrentUserService currentUserService)
        {
            _context = context;
            _userManager = userManager;
            _moneyConverter = moneyConverter;
            _orderService = orderService;
            _currentUserService = currentUserService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var user = await _currentUserService.EnsureAsync();

            var orders = await _orderService.GetOrdersAsync(user);

            return Ok(orders);
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var user = await _currentUserService.EnsureAsync();
            if (user == null) {
                return Unauthorized(ApiError.From(new Exception("Invalid user"), StatusCodes.Status401Unauthorized, HttpContext));
            }

           var result = await _orderService.GetOrderAsync(user, id);
            
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> MakeOrder([FromBody] CreateOrderDto order)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _currentUserService.EnsureAsync();
            if (user == null) return Unauthorized(ApiError.From(new Exception("Invalid user"), StatusCodes.Status401Unauthorized, HttpContext));

            var orderResult = await _orderService.MakeOrderAsync(order, user);
            return this.ToActionResult(orderResult);
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteOrderAsync(int id)
        {
            var user = await _currentUserService.EnsureAsync();
            if (user == null)
            {
                return NotFound(ApiError.From(new Exception("Invalid user"), StatusCodes.Status401Unauthorized, HttpContext));
            }

            var result = await _orderService.DeleteOrderAsync(user, id);

            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderDto dto)
        {
            var user = await _currentUserService.EnsureAsync();
            if (user == null)
            {
                return NotFound(ApiError.From(new Exception("Invalid user"), StatusCodes.Status401Unauthorized, HttpContext));
            }
            var result = await _orderService.UpdateOrderAsync(id, dto, user, default);

            return this.ToActionResult(result);
        }
        [Authorize]
        [HttpPut("AcceptOrRejectOrder")]
        public async Task<IActionResult> AcceptOrRejectOrder(int orderId, OrderStatus status, CancellationToken ct = default)
        {
            var user = await _currentUserService.EnsureAsync();
            if (user == null)
            {
                return NotFound(ApiError.From(new Exception("Invalid user"), StatusCodes.Status401Unauthorized, HttpContext));
            }
            var result = await _orderService.AcceptOrRejectOrderAsync(orderId, status, user, ct);
            return this.ToActionResult(result);
        }
        [Authorize]
        [HttpPut("CancellOrCorfirm")]
        public async Task<IActionResult> CancellOrCorfirmOrderReceipt(int orderId, OrderStatus status, CancellationToken ct = default)
        {
            var user = await _currentUserService.EnsureAsync();
            if (user == null)
            {
                return NotFound(ApiError.From(new Exception("Invalid user"), StatusCodes.Status401Unauthorized, HttpContext));
            }
            var result = await _orderService.CancelOrConfirmOrderReceiptAsync(orderId,user,status,ct);
            return this.ToActionResult(result);
        }


    }
}
