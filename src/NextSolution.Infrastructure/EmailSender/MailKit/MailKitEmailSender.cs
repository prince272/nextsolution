using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NextSolution.Core.Extensions.EmailSender;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NextSolution.Infrastructure.EmailSender.MailKit
{
    public class MailKitEmailSender : IEmailSender
    {
        private readonly IOptions<MailKitEmailOptions> _emailOptions;

        public MailKitEmailSender(IOptions<MailKitEmailOptions> emailOptions)
        {
            _emailOptions = emailOptions ?? throw new ArgumentNullException(nameof(emailOptions));
        }

        public async Task SendAsync(EmailAccount account, EmailMessage message)
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
                await client.ConnectAsync(_emailOptions.Value.SmtpHost, _emailOptions.Value.SmtpPort, SecureSocketOptions.SslOnConnect);
            }
            else
            {
                await client.ConnectAsync(_emailOptions.Value.SmtpHost, _emailOptions.Value.SmtpPort, SecureSocketOptions.StartTls);
            }

            await client.AuthenticateAsync(account.Username, account.Password);

            await client.SendAsync(minme);
            await client.DisconnectAsync(true);
        }

        public Task SendAsync(string account, EmailMessage message)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));
            if (message == null) throw new ArgumentNullException(nameof(message));


           if (!_emailOptions.Value.Accounts.TryGetValue(account, out var accountObject))
                throw new ArgumentException($"The specified account '{account}' was not found in the email options.", nameof(account));

            return SendAsync(accountObject, message);
        }
    }
}
