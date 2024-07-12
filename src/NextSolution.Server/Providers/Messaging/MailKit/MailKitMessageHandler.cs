using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace NextSolution.Server.Providers.Messaging.MailKit
{
    public class MailKitMessageHandler : IMessageHandler
    {
        private readonly IOptions<MailKitMessageSenderOptions> _messageSenderOptions;

        public MailKitMessageHandler(IOptions<MailKitMessageSenderOptions> messageSenderOptions)
        {
            _messageSenderOptions = messageSenderOptions;
        }

        public MessageChannel Channels => MessageChannel.Email;

        public async Task SenderAsync(MessageChannel channel, Message message, CancellationToken cancellationToken = default)
        {
            if (channel == MessageChannel.Email)
            {
                if (message == null) throw new ArgumentNullException(nameof(message));

                var account = _messageSenderOptions.Value.Accounts.TryGetValue(message.SenderId ?? _messageSenderOptions.Value.DefaultSenderId, out var acc)
                    ? acc : throw new KeyNotFoundException($"Account with SenderId '{message.SenderId ?? _messageSenderOptions.Value.DefaultSenderId}' not found.");

                var minme = new MimeMessage();
                minme.From.Add(new MailboxAddress(account.DisplayName, account.Email));

                foreach (var recipient in message.Recipients)
                {
                    minme.To.Add(new MailboxAddress(string.Empty, recipient));
                }

                minme.Subject = message.Subject;

                var builder = new BodyBuilder();
                builder.HtmlBody = message.Body;

                //foreach (var attachmentInfo in message.Attachments)
                //{
                //    builder.Attachments.Add(attachmentInfo.FileName, attachmentInfo.Content, ContentType.Parse(attachmentInfo.ContentType));
                //}

                minme.Body = builder.ToMessageBody();

                using var client = new SmtpClient();

                if (_messageSenderOptions.Value.UseSsl)
                {
                    await client.ConnectAsync(_messageSenderOptions.Value.Host, _messageSenderOptions.Value.Port, SecureSocketOptions.SslOnConnect, cancellationToken);
                }
                else
                {
                    await client.ConnectAsync(_messageSenderOptions.Value.Host, _messageSenderOptions.Value.Port, SecureSocketOptions.StartTls, cancellationToken);
                }

                await client.AuthenticateAsync(account.Username, account.Password, cancellationToken);

                await client.SendAsync(minme, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);
            }
        }
    }
}
