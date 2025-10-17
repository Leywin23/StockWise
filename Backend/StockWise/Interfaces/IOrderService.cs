using StockWise.Dtos.OrderDtos;
using StockWise.Helpers;
using StockWise.Models;

namespace StockWise.Interfaces
{
    public interface IOrderService
    {
        Task<ServiceResult<List<OrderListDto>>> GetOrdersAsync(AppUser user);
        Task<ServiceResult<OrderListDto>> GetOrderAsync(AppUser user, int id);
        Task<ServiceResult<OrderListDto>> MakeOrderAsync(CreateOrderDto order, AppUser user);
        Task<ServiceResult<OrderListDto>> DeleteOrderAsync(AppUser user, int id);
        Task<ServiceResult<OrderListDto>> UpdateOrderAsync(int id, UpdateOrderDto dto, AppUser user, CancellationToken ct = default);
        Task<ServiceResult<OrderDto>> AcceptOrRejectOrderAsync(int orderId, OrderStatus status, AppUser user);
        Task<ServiceResult<OrderDto>> CancellOrCorfirmOrderReceiptAsync(int orderId, AppUser user, OrderStatus status);
    }
}
