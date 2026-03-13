namespace TaskBoard.Api.Models
{
    public class FreelanceTask
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public record CreateTaskCommand(string Title, string Description);
}
