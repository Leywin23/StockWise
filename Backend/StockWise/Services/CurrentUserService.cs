using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Extensions;
using StockWise.Interfaces;
using StockWise.Models;
using System.Security.Claims;

namespace StockWise.Services
{
    public class CurrentUserService : ICurrentUserService
    {

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly StockWiseDb _context;
        private AppUser? _cached;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor, StockWiseDb context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }
        private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;
        public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;
        public string? UserId => Principal?.GetUserId();
        public async Task<AppUser?> GetAsync(CancellationToken ct = default)
        {
            if (!IsAuthenticated) return null;
            if (_cached is not null) return _cached;

            var id = UserId;
            if (string.IsNullOrEmpty(id)) return null;

            _cached = await _context.Users.Include(u => u.Company).FirstOrDefaultAsync(u => u.Id == id);

            return _cached;
        }

        public async Task<AppUser> EnsureAsync(CancellationToken ct = default)
        {
            if (!IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated");

            var user = await GetAsync(ct);
            return user ?? throw new UnauthorizedAccessException("User not found");
        }
    }
}
