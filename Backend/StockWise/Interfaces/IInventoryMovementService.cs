using StockWise.Dtos;
using StockWise.Dtos.InventoryMovementDtos;
using StockWise.Models;

namespace StockWise.Interfaces
{
    public interface IInventoryMovementService
    {
        Task<ICollection<InventoryMovement>> GetProductMovementHistoryAsync(int ProductId);
        Task<ServiceResult<InventoryMovement>> AddMovementAsync(InventoryMovementDto dto);
    }
}
