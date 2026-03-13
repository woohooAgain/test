namespace TaskBoard.Api.Deadlock
{
    public class Deadlock
    {
        public string Id { get; set; }
        public Deadlock() 
        {
            Id = Guid.NewGuid().ToString();
        }

        public async Task<string> GetId() 
        {
            await Task.Delay(1000);
            return Id;
        }
    }
}
