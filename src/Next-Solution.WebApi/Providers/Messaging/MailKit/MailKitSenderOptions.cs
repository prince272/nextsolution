namespace Next_Solution.WebApi.Providers.Messaging.MailKit
{
    public class MailKitSenderOptions
    {
        public string Host { get; set; } = null!;

        public int Port { get; set; }

        public bool UseSsl { get; set; }

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
