using HRestaurant.Configuration;
using HRestaurant.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HRestaurant.Infrastructure.BackgroundJobs;

public sealed class InventoryAlertBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly InventoryAlertSettings _settings;
    private readonly ILogger<InventoryAlertBackgroundService> _logger;

    public InventoryAlertBackgroundService(
        IServiceScopeFactory scopeFactory,
        InventoryAlertSettings settings,
        ILogger<InventoryAlertBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMinutes(_settings.CheckIntervalMinutes);
        using var timer = new PeriodicTimer(interval);
        _logger.LogInformation(
            "Inventory alert background service started with interval {IntervalMinutes} minutes.",
            _settings.CheckIntervalMinutes);

        do
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var service = scope.ServiceProvider.GetRequiredService<IInventoryAlertService>();
                var count = await service.ScanAsync(stoppingToken);
                _logger.LogInformation(
                    "Inventory alert scan completed. Processed {InventoryItemCount} items.", count);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception,
                    "Inventory alert scan failed. The background service will continue.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
