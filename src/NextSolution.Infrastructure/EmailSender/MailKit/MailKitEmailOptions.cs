using NextSolution.Core.Extensions.EmailSender;

namespace NextSolution.Infrastructure.EmailSender.MailKit
{
    public class MailKitEmailOptions
    {
        public string Host { get; set; } = default!;

        public int Port { get; set; }

        public bool EnableSsl { get; set; }

        private IDictionary<string, EmailAccount> accounts = new Dictionary<string, EmailAccount>();
        public IDictionary<string, EmailAccount> Accounts
        {
            get => new Dictionary<string, EmailAccount>(accounts, StringComparer.OrdinalIgnoreCase);
            set => accounts = value;
        }
    }
}