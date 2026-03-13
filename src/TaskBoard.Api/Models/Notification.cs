namespace TaskBoard.Api.Models
{

    public class Notification
    {
        public string Type { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.MinValue;

        public Notification() 
        {
        }

        public Notification(string type, string payload, DateTime createdAt)
        {
            Type = type;
            Payload = payload;
            CreatedAt = createdAt;
        }

        public override string ToString()
        {
            return $"Type:{Type} Payload:{Payload}";
        }
    }

    public record CreateNotificationCommand(string Type, string Payload);
}
