using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Dtos.InventoryMovementDtos;
using StockWise.Helpers;
using StockWise.Hubs;
using StockWise.Interfaces;
using StockWise.Models;

namespace StockWise.Services
{
    public class InventoryMovementService : IInventoryMovementService
    {
        private readonly StockWiseDb _context;
        private readonly IHubContext<StockHub> _hubContext;
        public InventoryMovementService(StockWiseDb context, IHubContext<StockHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<ServiceResult<InventoryMovement>> AddMovementAsync(InventoryMovementDto dto)
        {
            var product = await _context.CompanyProducts
            .FirstOrDefaultAsync(p => p.CompanyProductId == dto.CompanyProductId);

            if (product == null)
                return ServiceResult<InventoryMovement>.NotFound($"Couldn't find product with ID {dto.CompanyProductId}");

            var movement = new InventoryMovement
            {
                Date = dto.Date,
                Type = dto.Type,
                Quantity = dto.Quantity,
                CompanyProductId = dto.CompanyProductId,
                Comment = dto.Comment,
            };

            switch (movement.Type)
            {
                case MovementType.Inbound:
                    product.Stock += movement.Quantity;
                    break;

                case MovementType.Outbound:
                    if (product.Stock < movement.Quantity)
                        return ServiceResult<InventoryMovement>.BadRequest("Stock couldn't be below 0");

                    product.Stock -= movement.Quantity;
                    break;

                case MovementType.Adjustment:
                    product.Stock = movement.Quantity;
                    break;

                default:
                    return null;
            }


            _context.InventoryMovement.Add(movement);
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("StockUpdated", product.CompanyProductId, product.Stock);
            return ServiceResult<InventoryMovement>.Ok(movement);
        }

        public async Task<ServiceResult<ICollection<InventoryMovement>>> GetProductMovementHistoryAsync(int productId)
        {
            var movements = await _context.InventoryMovement
            .Where(m => m.CompanyProductId == productId)
            .ToListAsync();

            if (movements.Count == 0)
                return ServiceResult<ICollection<InventoryMovement>>.NotFound("No movements found for this product");

            return ServiceResult<ICollection<InventoryMovement>>.Ok(movements);
        }


    }
}
