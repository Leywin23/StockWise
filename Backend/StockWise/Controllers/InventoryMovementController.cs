using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Dtos.InventoryMovementDtos;
using StockWise.Helpers;
using StockWise.Hubs;
using StockWise.Interfaces;
using StockWise.Models;

namespace StockWise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryMovementController : ControllerBase
    {
        private readonly StockWiseDb _context;
        private readonly IHubContext<StockHub> _hubContext;
        private readonly IInventoryMovementService _inventoryMovementService;
        public InventoryMovementController(StockWiseDb context, IHubContext<StockHub> hubContext, IInventoryMovementService inventoryMovementService)
        {
            _context = context;
            _hubContext = hubContext;
            _inventoryMovementService = inventoryMovementService;
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
        [HttpPost]
        public async Task<IActionResult> AddMovement([FromBody] InventoryMovementDto dto)
        {
            var result = await _inventoryMovementService.AddMovementAsync(dto);

            if (!result.IsSuccess)
            {
                return result.Error switch
                {
                    ErrorKind.NotFound => NotFound(result.Message),
                    ErrorKind.BadRequest => BadRequest(result.Message),
                    ErrorKind.Unauthorized => Unauthorized(result.Message),
                    ErrorKind.Forbidden => Forbid(result.Message),
                    ErrorKind.Conflict => Conflict(result.Message),
                    _ => StatusCode(StatusCodes.Status500InternalServerError, result.Message)
                };
            }

            return Ok(result.Value);
        }
    }
}
