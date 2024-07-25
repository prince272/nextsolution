namespace NextSolution.Server.Providers.Messaging.MailKit
{
    public class TwilioMessageSenderOptions
    {
        public string AccountSid { get; set; } = null!;

        public string AuthToken { get; set; } = null!;

        public IDictionary<string, TwilioMessageAccount> Accounts { get; set; } = new Dictionary<string, TwilioMessageAccount>();
    }

    public class TwilioMessageAccount
    {
        public string PhoneNumber { get; set; } = null!;
    }
}
