using Microsoft.AspNetCore.Mvc;

namespace StockWise.Application.Interfaces
{
    public interface IExchangeRateProvider
    {
        Task<decimal> GetRateAsync(string fromCode, string toCode, CancellationToken ct = default);
    }
}
