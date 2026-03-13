using System.Threading.Channels;
using TaskBoard.Api.Interfaces;
using TaskBoard.Api.Models;

namespace TaskBoard.Api.Services
{
    public class NotificationService : INotificationChannel
    {
        private readonly ChannelWriter<Notification> _channelWriter;
        private readonly ChannelReader<Notification> _channelReader;

        public NotificationService() 
        {
            var channel = Channel.CreateUnbounded<Notification>();
            _channelWriter = channel.Writer;
            _channelReader = channel.Reader;
        }

        public void Complete()
        {
            _channelWriter.Complete();
        }

        public IAsyncEnumerable<Notification> ReadAllAsync(CancellationToken cancellationToken)
        {
            return _channelReader.ReadAllAsync(cancellationToken);
        }

        public ValueTask WriteAsync(Notification message, CancellationToken cancellationToken)
        {
            return _channelWriter.WriteAsync(message, cancellationToken);
        }
    }
}
