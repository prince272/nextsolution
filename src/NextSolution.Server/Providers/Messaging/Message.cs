namespace NextSolution.Server.Providers.Messaging
{
    public class Message
    {
        public string? SenderId { get; set; } = null!;

        public string? Subject { get; set; } = null!;

        public string? Body { get; set; }

        public string[] Recipients { get; set; } = Array.Empty<string>();
    }
}
