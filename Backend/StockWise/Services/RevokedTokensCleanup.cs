using Microsoft.EntityFrameworkCore;
using StockWise.Data;

namespace StockWise.Services
{
    public class RevokedTokensCleanup : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger<RevokedTokensCleanup> _log;

        public RevokedTokensCleanup(IServiceProvider sp, ILogger<RevokedTokensCleanup> log)
        {
            _sp = sp;
            _log = log;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();
                    var now = DateTime.UtcNow;
                    var expired = await db.RevokedTokens.Where(x => x.ExpiresAtUtc < now).ToListAsync(stoppingToken);
                    if (expired.Count > 0)
                    {
                        db.RevokedTokens.RemoveRange(expired);
                        await db.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "RevokedTokens cleanup failed");
                }

                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }
    }
}
