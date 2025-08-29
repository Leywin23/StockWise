using Microsoft.AspNetCore.Mvc;

namespace StockWise.Interfaces
{
    public interface IExchangeRateProvider
    {
        Task<Decimal> GetRateAsync(string fromCode, string toCode, CancellationToken ct = default);
    }
}
