using Microsoft.AspNetCore.Mvc;
using StockWise.Interfaces;
using System.Net.Http;

namespace StockWise.Services
{
    public class ApiExchangeRateProvider : IExchangeRateProvider
    {
        private readonly HttpClient _httpClient;
        public ApiExchangeRateProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://api.frankfurter.app/");
            _httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        }


        public async Task<Decimal> GetRateAsync(string fromCode, string toCode, CancellationToken ct = default)
        {
            var from = fromCode.Trim().ToUpperInvariant();
            var to = toCode.Trim().ToUpperInvariant();

            if (from == to) return 1m;

            var url = $"latest?amount=1&from={from}&to={to}";
            var resp = await _httpClient.GetFromJsonAsync<LatestResponse>(url, ct);

            if (resp?.Rates == null || !resp.Rates.TryGetValue(to, out var value)){
                throw new InvalidOperationException($"Course {from}=>{to} doesnt exist");
            }

            return value;
        }

        private sealed class LatestResponse
        {
            public string? Base { get; set; }
            public string? Date {  get; set; }
            public Dictionary<string,decimal> Rates = new Dictionary<string,decimal>();
        }
    }
}
