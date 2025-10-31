using StockWise.Application.Abstractions;
using StockWise.Application.Contracts.OrderDtos;
using StockWise.Models;

namespace StockWise.Application.Interfaces
{
    public interface IOrderService
    {
        Task<ServiceResult<List<OrderListDto>>> GetOrdersAsync(AppUser user);
        Task<ServiceResult<OrderListDto>> GetOrderAsync(AppUser user, int id);
        Task<ServiceResult<OrderListDto>> MakeOrderAsync(CreateOrderDto order, AppUser user);
        Task<ServiceResult<OrderListDto>> DeleteOrderAsync(AppUser user, int id);
        Task<ServiceResult<OrderListDto>> UpdateOrderAsync(int id, UpdateOrderDto dto, AppUser user, CancellationToken ct = default);
        Task<ServiceResult<OrderDto>> AcceptOrRejectOrderAsync(int orderId, OrderStatus status, AppUser user, CancellationToken ct = default);
        Task<ServiceResult<OrderDto>> CancelOrConfirmOrderReceiptAsync(int orderId, AppUser user, OrderStatus status, CancellationToken ct = default);
    }
}
