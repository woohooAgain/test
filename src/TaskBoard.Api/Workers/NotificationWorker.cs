using TaskBoard.Api.Interfaces;

namespace TaskBoard.Api.Workers
{
    public class NotificationWorker : BackgroundService
    {
        private readonly INotificationChannel _channel;
        private readonly ILogger<NotificationWorker> _logger;

        public NotificationWorker(INotificationChannel channel, ILogger<NotificationWorker> logger)
        {
            _channel = channel;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach(var n in _channel.ReadAllAsync(stoppingToken))
            {
                _logger.LogInformation("Received: Type:{Type} Payload:{Payload}", n.Type, n.Payload);
            }
        }
    }
}
