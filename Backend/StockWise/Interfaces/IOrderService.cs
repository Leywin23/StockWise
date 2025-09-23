using StockWise.Dtos.OrderDtos;
using StockWise.Models;

namespace StockWise.Interfaces
{
    public interface IOrderService
    {
        Task<OrderListDto> UpdateOrderAsync(int id, UpdateOrderDto dto, AppUser user);
    }
}
