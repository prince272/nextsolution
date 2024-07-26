namespace NextSolution._1.Server.Providers.Messaging
{
    [Flags]
    public enum MessageChannel
    {
        Email = 1 << 0,
        Sms = 1 << 1,
        WhatsApp = 1 << 2
    }
}
