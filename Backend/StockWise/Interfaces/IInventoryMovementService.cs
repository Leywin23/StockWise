using StockWise.Dtos.InventoryMovementDtos;
using StockWise.Models;
using StockWise.Response;

namespace StockWise.Interfaces
{
    public interface IInventoryMovementService
    {
        Task<ServiceResult<ICollection<InventoryMovement>>> GetProductMovementHistoryAsync(int ProductId);
        Task<ServiceResult<InventoryMovement>> AddMovementAsync(InventoryMovementDto dto);
    }
}
