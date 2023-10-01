using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NextSolution.Core.Extensions.EmailSender;

namespace NextSolution.Infrastructure.EmailSender.MailKit
{
    public class MailKitEmailSender : IEmailSender
    {
        private readonly IOptions<MailKitEmailOptions> _emailOptions;

        public MailKitEmailSender(IOptions<MailKitEmailOptions> emailOptions)
        {
            _emailOptions = emailOptions ?? throw new ArgumentNullException(nameof(emailOptions));
        }

        public async Task SendAsync(EmailAccount account, EmailMessage message, CancellationToken cancellationToken = default)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));
            if (message == null) throw new ArgumentNullException(nameof(account));

            var minme = new MimeMessage();
            minme.From.Add(new MailboxAddress(account.DisplayName, account.Email));

            foreach (var recipient in message.Recipients)
            {
                minme.To.Add(new MailboxAddress(string.Empty, recipient));
            }

            minme.Subject = message.Subject;

            var builder = new BodyBuilder();
            builder.HtmlBody = message.Body;

            foreach (var attachmentInfo in message.Attachments)
            {
                builder.Attachments.Add(attachmentInfo.FileName, attachmentInfo.Content, ContentType.Parse(attachmentInfo.ContentType));
            }

            minme.Body = builder.ToMessageBody();

            using var client = new SmtpClient();

            if (_emailOptions.Value.EnableSsl)
            {
                await client.ConnectAsync(_emailOptions.Value.Host, _emailOptions.Value.Port, SecureSocketOptions.SslOnConnect, cancellationToken);
            }
            else
            {
                await client.ConnectAsync(_emailOptions.Value.Host, _emailOptions.Value.Port, SecureSocketOptions.StartTls, cancellationToken);
            }

            await client.AuthenticateAsync(account.Username, account.Password, cancellationToken);

            await client.SendAsync(minme, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }

        public Task SendAsync(string account, EmailMessage message, CancellationToken cancellationToken = default)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));
            if (message == null) throw new ArgumentNullException(nameof(message));


            if (!_emailOptions.Value.Accounts.TryGetValue(account, out var accountObject))
                throw new ArgumentException($"The specified account '{account}' was not found in the email options.", nameof(account));

            return SendAsync(accountObject, message, cancellationToken);
        }
    }
}
