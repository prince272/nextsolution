namespace NextSolution.WebApi.Providers.Messaging
{
    public class Message
    {
        public string? AccountId { get; set; } = null!;

        public string? Subject { get; set; } = null!;

        public string? Body { get; set; }

        public string[] Recipients { get; set; } = Array.Empty<string>();
    }
}
