using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using StockWise.Data;
using StockWise.Hubs;
using StockWise.Models;
using Microsoft.EntityFrameworkCore;
using StockWise.Dtos.InventoryMovementDtos;
using StockWise.Interfaces;

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
        public async Task<IActionResult> AddMovement([FromBody] InventoryMovementDto dto)
        {
            var result = await _inventoryMovementService.AddMovementAsync(dto);
            if (!result.Success)
            {
                if(result.ErrorMessage.Contains("Couldn't find product"))
                    return NotFound(result.ErrorMessage);

                return BadRequest(result.ErrorMessage);
            }

            return Ok(result.Data);
        }
    }
}
