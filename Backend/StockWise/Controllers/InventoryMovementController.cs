using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StockWise.Application.Contracts.InventoryMovementDtos;
using StockWise.Application.Interfaces;
using StockWise.Extensions;
using StockWise.Hubs;
using StockWise.Infrastructure.Persistence;
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
        [Authorize]
        public async Task<IActionResult> GetProductMovementHistory(int ProductId)
        {
            
            var product = await _inventoryMovementService.GetProductMovementHistoryAsync(ProductId);

            return this.ToActionResult(product);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddMovement([FromBody] InventoryMovementDto dto)
        {
            var result = await _inventoryMovementService.AddMovementAsync(dto);

            return this.ToActionResult(result);
        }

    }
}
