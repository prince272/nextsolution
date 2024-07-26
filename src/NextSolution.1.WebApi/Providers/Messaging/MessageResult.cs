namespace NextSolution._1.Server.Providers.Messaging
{
    public class MessageResult
    {
        public IDictionary<string, bool> Recipients { get; set; } = new Dictionary<string, bool>();

        public MessageStatus Status => Recipients.Values.All(x => x)
            ? MessageStatus.Sent : Recipients.Values.Any(x => x)
            ? MessageStatus.PartlySent
            : MessageStatus.Failed;
    }

    public enum MessageStatus
    {
        Sent,    
        PartlySent,
        Failed
    }
}
