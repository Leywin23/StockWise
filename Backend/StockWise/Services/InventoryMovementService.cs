using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Dtos.InventoryMovementDtos;
using StockWise.Dtos.OrderDtos;
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
        private readonly ICurrentUserService _currentUserService;
        public InventoryMovementService(StockWiseDb context, IHubContext<StockHub> hubContext, ICurrentUserService currentUserService)
        {
            _context = context;
            _hubContext = hubContext;
            _currentUserService = currentUserService;
        }

        public async Task<ServiceResult<InventoryMovement>> AddMovementAsync(InventoryMovementDto dto)
        {
            var user = await _currentUserService.EnsureAsync();
            if(user == null)
            {
                return ServiceResult<InventoryMovement>.Unauthorized("User not found");
            }
            if (user.CompanyMembershipStatus != CompanyMembershipStatus.Approved)
            {
                return ServiceResult<InventoryMovement>.Unauthorized("You have to be approved by a manager to use this functionality");
            }

            if(user.CompanyId == null)
            {
                return ServiceResult<InventoryMovement>.Unauthorized("User does not belong to any company");
            }
            var product = await _context.CompanyProducts
            .FirstOrDefaultAsync(p => p.CompanyProductId == dto.CompanyProductId);

            if (product == null)
                return ServiceResult<InventoryMovement>.NotFound($"Couldn't find product with ID {dto.CompanyProductId}");

            if (product.CompanyId != user.CompanyId)
            {
                return ServiceResult<InventoryMovement>.Forbidden($"You can not add movement to product from outside your company");
            }

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
            var user = await _currentUserService.EnsureAsync();
            if (user.CompanyMembershipStatus != CompanyMembershipStatus.Approved)
            {
                return ServiceResult<ICollection<InventoryMovement>>.Unauthorized("You have to be approved by a manager to use this functionality");
            }
            if(user.CompanyId == null)
            {
                return ServiceResult<ICollection<InventoryMovement>>.Unauthorized("The user does not belong to any company");
            }
            var companyProduct = await _context.CompanyProducts.FirstOrDefaultAsync(cp=>cp.CompanyProductId == productId);
            if (companyProduct == null)
            {
                return ServiceResult<ICollection<InventoryMovement>>.NotFound($"Unable to find company product with Id: {productId}");
            }
            if(companyProduct.CompanyId != user.CompanyId)
            {
                return ServiceResult<ICollection<InventoryMovement>>.Forbidden("The product does not belong to your company");
            }

            var movements = await _context.InventoryMovement
            .Where(m => m.CompanyProductId == productId && m.CompanyProduct.CompanyId == user.CompanyId)
            .ToListAsync();

            if (movements.Count == 0)
                return ServiceResult<ICollection<InventoryMovement>>.NotFound("No movements found for this product");

            return ServiceResult<ICollection<InventoryMovement>>.Ok(movements);
        }


    }
}
