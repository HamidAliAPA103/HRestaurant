using HRestaurant.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HRestaurant.Infrastructure.BackgroundJobs;

public sealed class ReservationEmailBackgroundService
    : BackgroundService
{
    private readonly ReservationEmailQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReservationEmailBackgroundService> _logger;

    public ReservationEmailBackgroundService(
        ReservationEmailQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<ReservationEmailBackgroundService> logger)
    {
        ArgumentNullException.ThrowIfNull(queue);
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(logger);

        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        await foreach (var message in _queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await using var scope =
                    _scopeFactory.CreateAsyncScope();
                var sender = scope.ServiceProvider
                    .GetRequiredService<IReservationEmailSender>();

                await sender.SendAsync(message, stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Reservation confirmation email failed. "
                    + "ConfirmationCode: {ConfirmationCode}",
                    message.ConfirmationCode);
            }
        }
    }
}
