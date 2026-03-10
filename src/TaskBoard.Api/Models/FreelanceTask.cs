namespace TaskBoard.Api.Models
{
    public class FreelanceTask
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public record CreateTaskCommand(string Title, string Description);
}
