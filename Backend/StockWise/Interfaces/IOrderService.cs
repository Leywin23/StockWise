using StockWise.Dtos.OrderDtos;
using StockWise.Helpers;
using StockWise.Models;

namespace StockWise.Interfaces
{
    public interface IOrderService
    {
        Task<ServiceResult<OrderListDto>> MakeOrderAsync(CreateOrderDto order, AppUser user);
        Task<ServiceResult<OrderListDto>> UpdateOrderAsync(int id, UpdateOrderDto dto, AppUser user, CancellationToken ct = default);
    }
}
