
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StockWise.Infrastructure.Persistence;

namespace StockWise.Infrastructure.HostedService
{
    public class UnverifiedCompanyCleanup : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger<UnverifiedCompanyCleanup> _log;
        private static readonly TimeSpan Interval = TimeSpan.FromHours(2);
        private static readonly TimeSpan Lifetime = TimeSpan.FromHours(2);

        public UnverifiedCompanyCleanup(IServiceProvider sp, ILogger<UnverifiedCompanyCleanup> log)
        {
            _sp = sp;
            _log = log;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(Interval);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = _sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<StockWiseDb>();

                    var cutoff = DateTime.UtcNow - Lifetime;

                    var deletedCount = await db.Companies
                        .Where(c => !c.Verified && c.CreatedAt < cutoff)
                        .ExecuteDeleteAsync(stoppingToken);

                    if (deletedCount > 0)
                    {
                        _log.LogInformation(
                            "UnverifiedCompanyCleanup removed {Count} companies older than {Lifetime} (cutoff: {Cutoff})",
                            deletedCount, Lifetime, cutoff);
                    }
                }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(ex, "UnverifiedCompanyCleanup failed.");
                }
            }
        }
    }
}
