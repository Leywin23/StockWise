using StockWise.Dtos.InventoryMovementDtos;
using StockWise.Helpers;
using StockWise.Models;

namespace StockWise.Interfaces
{
    public interface IInventoryMovementService
    {
        Task<ServiceResult<ICollection<InventoryMovement>>> GetProductMovementHistoryAsync(int ProductId);
        Task<ServiceResult<InventoryMovement>> AddMovementAsync(InventoryMovementDto dto);
    }
}
