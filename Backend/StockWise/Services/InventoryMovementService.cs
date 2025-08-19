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
                return ServiceResult<InventoryMovement>.Fail($"Couldn't find product with ID {dto.CompanyProductId}");

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
                        return ServiceResult<InventoryMovement>.Fail("Stock couldn't be below 0");

                    product.Stock -= movement.Quantity;
                    break;

                case "adjustment":
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

        public async Task<ICollection<InventoryMovement>> GetProductMovementHistoryAsync(int ProductId)
        {
            var product = await _context.CompanyProducts
                  .Include(p => p.InventoryMovements)
                  .FirstOrDefaultAsync(p => p.CompanyProductId == ProductId);

            if (product == null) return null;

            return product.InventoryMovements;
        }


    }
}
