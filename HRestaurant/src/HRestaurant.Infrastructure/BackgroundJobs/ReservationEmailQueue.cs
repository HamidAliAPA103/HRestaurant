using System.Threading.Channels;
using HRestaurant.Services.Interfaces;

namespace HRestaurant.Infrastructure.BackgroundJobs;

public sealed class ReservationEmailQueue : IReservationEmailQueue
{
    private readonly Channel<ReservationEmailMessage> _channel =
        Channel.CreateBounded<ReservationEmailMessage>(
            new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            });

    public ValueTask QueueAsync(
        ReservationEmailMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        return _channel.Writer.WriteAsync(
            message,
            cancellationToken);
    }

    internal IAsyncEnumerable<ReservationEmailMessage> ReadAllAsync(
        CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
