
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
        private static readonly TimeSpan Interval = TimeSpan.FromHours(1);
        private static readonly TimeSpan Lifetime = TimeSpan.FromHours(1);

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

                    var oldCompanyIds = await db.Companies
                        .Where(c => !c.Verified && c.CreatedAt < cutoff)
                        .Where(c => !db.Orders.Any(o => o.BuyerId == c.Id || o.SellerId == c.Id))
                        .Select(c => c.Id)
                        .ToListAsync(stoppingToken);

                    if (oldCompanyIds.Count == 0)
                        continue;

                    var deletedUsers = await db.Users
                        .Where(u => u.CompanyId != null && oldCompanyIds.Contains(u.CompanyId.Value))
                        .ExecuteDeleteAsync(stoppingToken);

                    var deletedProducts = await db.CompanyProducts
                        .Where(cp => oldCompanyIds.Contains(cp.CompanyId))
                        .ExecuteDeleteAsync(stoppingToken);

                    var deletedCompanies = await db.Companies
                        .Where(c => oldCompanyIds.Contains(c.Id))
                        .ExecuteDeleteAsync(stoppingToken);

                    if (deletedCompanies > 0)
                    {
                        _log.LogInformation(
                            "UnverifiedCompanyCleanup removed {Companies} companies, {Users} users and {Products} products older than {Lifetime} (cutoff: {Cutoff})",
                            deletedCompanies, deletedUsers, deletedProducts, Lifetime, cutoff);
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
