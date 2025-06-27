using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using StockWise.Data;
using StockWise.Dtos;
using StockWise.Hubs;
using StockWise.Models;

namespace StockWise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryMovementController : ControllerBase
    {
        private readonly StockWiseDb _context;
        private readonly IHubContext<StockHub> _hubContext;
        public InventoryMovementController(StockWiseDb context, IHubContext<StockHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> AddMovement([FromBody] InventoryMovementDto dto)
        {
            var product = await _context.products.FindAsync(dto.ProductId);

            if (product == null) {
                return NotFound($"Couldn't find a product with id {dto.ProductId}");
            }

            var movement = new InventoryMovement
            {
                Date = dto.Date,
                Type = dto.Type.ToLower(),
                Quantity = dto.Quantity,
                ProductId = dto.ProductId,
                Comment = dto.Comment,
            };

            switch (movement.Type) {
                case "inbound":
                    product.Stock += movement.Quantity;
                    break;
                case "outbound":
                    product.Stock -= movement.Quantity;
                    break;
                case "adjustment":
                    product.Stock = movement.Quantity;
                    break;
                default:
                    return BadRequest("Invalid movement type");
            }
            _context.inventoryMovement.Add(movement);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("StockUpdated", product.ProductId, product.Stock);

            return Ok(movement);
        }
    }
}
