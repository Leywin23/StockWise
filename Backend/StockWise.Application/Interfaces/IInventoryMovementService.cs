using StockWise.Application.Abstractions;
using StockWise.Application.Contracts.InventoryMovementDtos;
using StockWise.Models;

namespace StockWise.Application.Interfaces
{
    public interface IInventoryMovementService
    {
        Task<ServiceResult<ICollection<InventoryMovementDto>>> GetProductMovementHistoryAsync(int ProductId);
        Task<ServiceResult<InventoryMovementDto>> AddMovementAsync(InventoryMovementDto dto);
    }
}
