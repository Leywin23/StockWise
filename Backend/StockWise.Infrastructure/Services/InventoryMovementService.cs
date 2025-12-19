using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StockWise.Application.Abstractions;
using StockWise.Application.Contracts.InventoryMovementDtos;
using StockWise.Application.Interfaces;
using StockWise.Hubs;
using StockWise.Infrastructure.Persistence;
using StockWise.Models;

namespace StockWise.Infrastructure.Services
{
    public class InventoryMovementService : IInventoryMovementService
    {
        private readonly StockWiseDb _context;
        private readonly IHubContext<StockHub> _hubContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        public InventoryMovementService(StockWiseDb context, IHubContext<StockHub> hubContext, ICurrentUserService currentUserService, IMapper mapper)
        {
            _context = context;
            _hubContext = hubContext;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<ServiceResult<InventoryMovementDto>> AddMovementAsync(InventoryMovementDto dto)
        {
            var user = await _currentUserService.EnsureAsync();
            if(user == null)
            {
                return ServiceResult<InventoryMovementDto>.Unauthorized("User not found");
            }
            if (user.CompanyMembershipStatus != CompanyMembershipStatus.Approved)
            {
                return ServiceResult<InventoryMovementDto>.Unauthorized("You have to be approved by a manager to use this functionality");
            }

            if(user.CompanyId == null)
            {
                return ServiceResult<InventoryMovementDto>.Unauthorized("User does not belong to any company");
            }
            var product = await _context.CompanyProducts
            .FirstOrDefaultAsync(p => p.CompanyProductId == dto.CompanyProductId);

            if (product == null)
                return ServiceResult<InventoryMovementDto>.NotFound($"Couldn't find product with ID {dto.CompanyProductId}");

            if (product.CompanyId != user.CompanyId)
            {
                return ServiceResult<InventoryMovementDto>.Forbidden($"You can not add movement to product from outside your company");
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
                        return ServiceResult<InventoryMovementDto>.BadRequest("Stock couldn't be below 0");

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
            
            var inventoryMovementDto = _mapper.Map<InventoryMovementDto>(movement);
            return ServiceResult<InventoryMovementDto>.Ok(inventoryMovementDto);
        }

        public async Task<ServiceResult<ICollection<InventoryMovementDto>>> GetProductMovementHistoryAsync(int productId)
        {
            var user = await _currentUserService.EnsureAsync();
            if (user.CompanyMembershipStatus != CompanyMembershipStatus.Approved)
            {
                return ServiceResult<ICollection<InventoryMovementDto>>.Unauthorized("You have to be approved by a manager to use this functionality");
            }
            if(user.CompanyId == null)
            {
                return ServiceResult<ICollection<InventoryMovementDto>>.Unauthorized("The user does not belong to any company");
            }
            var companyProduct = await _context.CompanyProducts.FirstOrDefaultAsync(cp=>cp.CompanyProductId == productId);
            if (companyProduct == null)
            {
                return ServiceResult<ICollection<InventoryMovementDto>>.NotFound($"Unable to find company product with Id: {productId}");
            }
            if(companyProduct.CompanyId != user.CompanyId)
            {
                return ServiceResult<ICollection<InventoryMovementDto>>.Forbidden("The product does not belong to your company");
            }

            var movements = await _context.InventoryMovement
            .Where(m => m.CompanyProductId == productId && m.CompanyProduct.CompanyId == user.CompanyId)
            .ToListAsync();

            if (movements.Count == 0)
                return ServiceResult<ICollection<InventoryMovementDto>>.NotFound("No movements found for this product");

            var movementsDto = _mapper.Map<List<InventoryMovementDto>>(movements);
            return ServiceResult<ICollection<InventoryMovementDto>>.Ok(movementsDto);
        } 


    }
}
