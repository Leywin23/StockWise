using StockWise.Models;

namespace StockWise.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(AppUser user);
    }
}
