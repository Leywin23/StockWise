using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using StockWise.Data;
using StockWise.Hubs;
using StockWise.Models;
using Microsoft.EntityFrameworkCore;
using StockWise.Dtos.InventoryMovementDtos;

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


        [HttpGet("{ProductId:int}")]
        public async Task<IActionResult> GetProductMovementHistory(int ProductId)
        {
            var product = await _context.CompanyProducts
                  .Include(p => p.InventoryMovements)
                  .FirstOrDefaultAsync(p => p.CompanyProductId == ProductId);

            if (product == null) return NotFound();


            return Ok(product.InventoryMovements);
        }

        [HttpPost]
        public async Task<IActionResult> AddMovement([FromBody] InventoryMovementDto dto)
        {
            var product = await _context.CompanyProducts
    .FirstOrDefaultAsync(p => p.CompanyProductId == dto.CompanyProductId);

            if (product == null)
                return NotFound($"Couldn't find product with ID {dto.CompanyProductId}");

            var movement = new InventoryMovement
            {
                Date = dto.Date,
                Type = dto.Type.ToLower(),
                Quantity = dto.Quantity,
                CompanyProductId = dto.CompanyProductId,
                Comment = dto.Comment,
            };

            switch (movement.Type?.ToLowerInvariant())
            {
                case "inbound":
                    product.Stock += movement.Quantity;
                    break;

                case "outbound":
                    if (product.Stock < movement.Quantity)
                        return BadRequest("Stock couldn't be below 0");

                    product.Stock -= movement.Quantity;
                    break;

                case "adjustment":
                    product.Stock = movement.Quantity;
                    break;

                default:
                    return BadRequest("Invalid movement type");
            }
            _context.InventoryMovement.Add(movement);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("StockUpdated", product.CompanyProductId, product.Stock);

            return Ok(movement);
        }
    }
}
