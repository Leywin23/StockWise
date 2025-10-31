using StockWise.Models;

namespace StockWise.Application.Interfaces
{
    public interface ITokenService
    {
        Task<string> CreateToken(AppUser user);
    }
}
