using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;
using StockWise.Interfaces;

namespace StockWise.Services
{
    public class CachedExchangeRateProvider : IExchangeRateProvider
    {
        private readonly IExchangeRateProvider _inner;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CachedExchangeRateProvider> _logger;
        private readonly MemoryCacheEntryOptions _options;

        public CachedExchangeRateProvider(IExchangeRateProvider inner, IMemoryCache cache, ILogger<CachedExchangeRateProvider> logger, TimeSpan ttl)
        {
            _inner = inner;
            _cache = cache;
            _logger = logger;
            _options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl,
            };
        }

        public async Task<decimal> GetRateAsync(string fromCode, string toCode, CancellationToken ct = default)
        {
            var from = fromCode.Trim().ToUpperInvariant();
            var to = toCode.Trim().ToUpperInvariant();

            if (from == to) return 1m;

            var key = $"{from},{to}";

            if (_cache.TryGetValue(key, out decimal value)) {
                _logger.LogInformation("[CACHE HIT] {Key} => {Rate}", key, value);
                return value; 
            }

            _logger.LogInformation("[CACHE MISS] {Key} – I download from an internal provider…", key);

            var rate = await _inner.GetRateAsync(from, to, ct);

            _cache.Set(key, rate, _options);

            _logger.LogInformation("[CACHE SET] {Key} => {Rate}, TTL={Ttl}", key, rate, _options.AbsoluteExpirationRelativeToNow);
            return rate;


        }
    }
}
