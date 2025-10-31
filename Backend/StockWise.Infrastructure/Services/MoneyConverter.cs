using StockWise.Application.Interfaces;
using StockWise.Models;

namespace StockWise.Infrastructure.Services
{
    public class MoneyConverter
    {
        private readonly IExchangeRateProvider _rates;
        public MoneyConverter(IExchangeRateProvider rates)
        {
            _rates = rates;
        }

        public async Task<Money> ConvertAsync(Money source, string toCode) {

            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (source.Currency is null || string.IsNullOrWhiteSpace(source.Currency.Code))
                throw new InvalidOperationException("Source Money has no currency code.");
            if (string.IsNullOrWhiteSpace(toCode))
                throw new ArgumentException("Target currency code is required.", nameof(toCode));

            var from = source.Currency.Code.Trim().ToUpperInvariant();
            var to = toCode.Trim().ToUpperInvariant();

            if (from == to) return source;

            var rate = await _rates.GetRateAsync(from, to);
            var result = Math.Round(source.Amount * rate, 2);

            return Money.Of(result, to);
        }
    }
}
