using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace NextSolution._1.Server.Providers.Messaging.MailKit
{
    public class MailKitMessageHandler : IMessageHandler
    {
        private readonly IOptions<MailKitMessageSenderOptions> _messageSenderOptions;
        private readonly ILogger<MailKitMessageHandler> _logger;

        public MailKitMessageHandler(IOptions<MailKitMessageSenderOptions> messageSenderOptions, ILogger<MailKitMessageHandler> logger)
        {
            _messageSenderOptions = messageSenderOptions ?? throw new ArgumentNullException(nameof(messageSenderOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public MessageChannel Channels => MessageChannel.Email;

        public Task<MessageResult> SendAsync(MessageChannel channel, Message message, CancellationToken cancellationToken = default)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            return channel switch
            {
                MessageChannel.Email => SendEmailAsync(message, cancellationToken),
                _ => throw new NotSupportedException($"Channel '{channel}' is not supported.")
            };
        }

        public async Task<MessageResult> SendEmailAsync(Message message, CancellationToken cancellationToken = default)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var account = _messageSenderOptions.Value.Accounts.TryGetValue(message.AccountId ?? "Default", out var acc)
                ? acc : throw new InvalidOperationException($"Account with ID '{message.AccountId}' was not found.");

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

            var result = new MessageResult();
            result.Recipients = message.Recipients.ToDictionary(recipient => recipient, _ => false);

            foreach (var recipientEmail in message.Recipients)
            {
                var minme = new MimeMessage();
                minme.Subject = message.Subject;
                minme.From.Add(new MailboxAddress(account.DisplayName, account.Email));
                minme.To.Add(new MailboxAddress(string.Empty, recipientEmail));

                var builder = new BodyBuilder();
                builder.HtmlBody = message.Body;

                //foreach (var attachmentInfo in message.Attachments)
                //{
                //    builder.Attachments.Add(attachmentInfo.FileName, attachmentInfo.Content, ContentType.Parse(attachmentInfo.ContentType));
                //}

                minme.Body = builder.ToMessageBody();

                try
                {
                    await client.SendAsync(minme, cancellationToken);

                    result.Recipients[recipientEmail] = true;
                }
                catch (Exception ex)
                {
                    // Log the exception
                    _logger.LogError(ex, "Failed to send email to {RecipientEmail}.", recipientEmail);
                }
            }

            await client.DisconnectAsync(true, cancellationToken);

            return result;
        }
    }
}