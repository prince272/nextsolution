namespace NextSolution.Server.Providers.Messaging.MailKit
{
    public class MailKitMessageSenderOptions
    {
        public string Host { get; set; } = null!;

        public int Port { get; set; }

        public bool UseSsl { get; set; }

        public string DefaultSenderId { get; set; } = null!;

        public IDictionary<string, MailKitMessageAccount> Accounts { get; set; } = new Dictionary<string, MailKitMessageAccount>();
    }

    public class MailKitMessageAccount
    {
        public string? DisplayName { get; set; }
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
