using StockWise.Interfaces;
using StockWise.Models;

namespace StockWise.Services
{
    public class MoneyConverter
    {
        private readonly IExchangeRateProvider _rates;
        public MoneyConverter(IExchangeRateProvider rates)
        {
            _rates = rates;
        }

        public async Task<Money> ConvertAsync(Money source, string toCode) {
        
            var to = toCode.Trim().ToUpperInvariant();

            if (to == source.currency.CurrencyCode) return source;

            var rate = await _rates.GetRateAsync(source.currency.CurrencyCode, to);

            var result = Math.Round(source.amount * rate, 2);

            return Money.Of(result, to);

        }
    }
}
