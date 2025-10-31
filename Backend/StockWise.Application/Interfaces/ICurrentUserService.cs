using StockWise.Models;

namespace StockWise.Application.Interfaces
{
    public interface ICurrentUserService
    {
        bool IsAuthenticated { get; }
        string? UserId { get; }

        Task<AppUser> EnsureAsync(CancellationToken ct = default);
        Task<AppUser?> GetAsync(CancellationToken ct = default);
    }
}