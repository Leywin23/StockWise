using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using StockWise.Application.Interfaces;
using StockWise.Extensions;
using StockWise.Infrastructure.Persistence;
using StockWise.Infrastructure.Services;
using StockWise.Models;
using System.Text.Json;

namespace StockWise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EanController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly StockWiseDb _context;
        private readonly IEanService _eanService;

        public EanController(IHttpClientFactory httpClient, StockWiseDb context, IEanService eanService)
        {
            _httpClient = httpClient;
            _context = context; 
            _eanService = eanService;
        }

        [HttpGet("{ean}")]
        public async Task<IActionResult> GetProductByEan(string ean, CancellationToken ct = default)
        {
            var result = await _eanService.GetAndCreateProductByEanAsync(ean, ct);
            return this.ToActionResult(result);
        }

    }
}
