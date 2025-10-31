using StockWise.Application.Interfaces;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace StockWise.Infrastructure.Services
{
    public class ApiExchangeRateProvider : IExchangeRateProvider
    {
        private readonly HttpClient _httpClient;

        public ApiExchangeRateProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://api.frankfurter.dev/");
            _httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        }

        public async Task<decimal> GetRateAsync(string fromCode, string toCode, CancellationToken ct = default)
        {
            var from = fromCode.Trim().ToUpperInvariant();
            var to = toCode.Trim().ToUpperInvariant();

            if (from == to) return 1m;

            var url = $"v1/latest?base={from}&symbols={to}";
            var resp = await _httpClient.GetFromJsonAsync<LatestResponse>(url, ct);

            if (resp?.Rates == null || !resp.Rates.TryGetValue(to, out var value))
            {
                throw new InvalidOperationException($"Rate {from} => {to} not found in response.");
            }

            return value;
        }

        private sealed class LatestResponse
        {
            [JsonPropertyName("base")]
            public string? Base { get; set; }

            [JsonPropertyName("date")]
            public string? Date { get; set; }

            [JsonPropertyName("rates")]
            public Dictionary<string, decimal> Rates { get; set; } = new();
        }
    }
}
