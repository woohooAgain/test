using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using TaskBoard.Api.Models;
using TaskBoard.Api.Services;
using TaskBoard.Api.Workers;

namespace TaskBoard.UnitTests.Notifications
{
    public class NotificationWorkerTests
    {
        [Fact]
        public async Task ExecuteAsync_WhenNotificationConsumed_LogIt()
        {
            var notificationService = new NotificationService();
            var logger = new FakeLogger<NotificationWorker>();
            var worker = new NotificationWorker(notificationService, logger);

            var notification = new Notification
            {
                Type = "test type",
                Payload = "test payload",
                CreatedAt = DateTime.UtcNow,
            };

            var cts = new CancellationTokenSource();
            var workerTask = worker.StartAsync(cts.Token);

            await notificationService.WriteAsync(notification, cts.Token);
            await Task.Delay(100);

            Assert.Equal(LogLevel.Information, logger.LatestRecord.Level);
            Assert.Equal("Received: Type:test type Payload:test payload", logger.LatestRecord.Message);
        }
    }
}
