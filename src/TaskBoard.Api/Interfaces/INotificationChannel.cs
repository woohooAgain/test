using TaskBoard.Api.Models;

namespace TaskBoard.Api.Interfaces
{
    public interface INotificationChannel
    {
        ValueTask WriteAsync(Notification message, CancellationToken cancellationToken);
        IAsyncEnumerable<Notification> ReadAllAsync(CancellationToken cancellationToken);
        void Complete(); // сигнал что записей больше не будет
    }
}
