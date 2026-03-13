using TaskBoard.Api.Models;
using TaskBoard.Api.Services;

namespace TaskBoard.UnitTests.Notifications
{
    public class NotificationChannelTests
    {
        [Fact]
        public async Task WriteAsync_ThenReadAll_ReturnsAllNotifications()
        {
            var channel = new NotificationService();
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            await channel.WriteAsync(new Notification("TaskCreated", "payload-1", DateTime.UtcNow), token);
            await channel.WriteAsync(new Notification("TaskCreated", "payload-2", DateTime.UtcNow), token);
            await channel.WriteAsync(new Notification("TaskCreated", "payload-3", DateTime.UtcNow), token);
            channel.Complete(); // ← закрыли, ReadAllAsync завершится

            var result = new List<Notification>();
            await foreach (var n in channel.ReadAllAsync(token))
                result.Add(n);

            Assert.Equal(3, result.Count);
        }
    }
}
