using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PrismaApi.Application.Interfaces.Services;

namespace PrismaApi.Application.BackgroundServices;

public class TableCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TableCleanupService> _logger;
    private readonly TimeSpan _delay = TimeSpan.FromMinutes(10);

    public TableCleanupService(IServiceScopeFactory serviceScopeFactory, ILogger<TableCleanupService> logger)
    {
        _scopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct = default)
    {
        try
        {
            await CleanupLoopAsync(ct);
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("TableCleanupService is stopping due to cancellation.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred in TableCleanupService.");
        }
    }

    /// <summary>
    /// This is a temporary solution to clean up excess discrete probabilities and utilities until we implement a more robust solution in the future. It continuously runs a loop that cleans up excess discrete probabilities and utilities every 10 or so minutes. This is necessary because currently we do not have a way to automatically clean up excess discrete probabilities and utilities.
    /// </summary>
    private async Task CleanupLoopAsync(CancellationToken ct = default)
    {
        while (true)
        {
            try
            {
                var startTime = DateTime.UtcNow;
                using var scope = _scopeFactory.CreateScope();
                var tableRebuildingService = scope.ServiceProvider.GetRequiredService<ITableRebuildingService>();
                await tableRebuildingService.RemoveExcessDiscreteProbabilities(ct);
                await tableRebuildingService.RemoveExcessDiscreteUtilities(ct);
                var endTime = DateTime.UtcNow;
                // logging the duration of the cleanup process, 
                // if the duration in the future becomes consistantly short it implyies that our fixes for excess discrete probabilities and utilities are working and we can remove this background service
                _logger.LogInformation("Table cleanup completed successfully in {Duration} ms.", (endTime - startTime).TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in CleanupLoopAsync.");
            }
            await Task.Delay(_delay, ct);
            ct.ThrowIfCancellationRequested();
        }
    }
}