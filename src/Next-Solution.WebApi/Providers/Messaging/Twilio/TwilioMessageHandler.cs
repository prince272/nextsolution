using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Next_Solution.WebApi.Providers.Messaging.Twilio
{
    public class TwilioMessageHandler : IMessageHandler
    {
        private readonly IOptions<TwilioMessageSenderOptions> _messageSenderOptions;
        private readonly ILogger<TwilioMessageHandler> _messageSenderLogger;

        public TwilioMessageHandler(IOptions<TwilioMessageSenderOptions> messageSenderOptions, ILogger<TwilioMessageHandler> messageSenderLogger)
        {
            _messageSenderOptions = messageSenderOptions ?? throw new ArgumentNullException(nameof(messageSenderOptions));
            _messageSenderLogger = messageSenderLogger ?? throw new ArgumentNullException(nameof(messageSenderLogger));
        }

        public MessageChannel Channels => MessageChannel.Sms;

        public Task<MessageResult> SendAsync(MessageChannel channel, Message message, CancellationToken cancellationToken = default)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            return channel switch
            {
                MessageChannel.Sms => SendSmsAsync(message, cancellationToken),
                _ => throw new NotSupportedException($"Channel '{channel}' is not supported.")
            };
        }

        public async Task<MessageResult> SendSmsAsync(Message message, CancellationToken cancellationToken = default)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            TwilioClient.Init(_messageSenderOptions.Value.AccountSid, _messageSenderOptions.Value.AuthToken);

            var account = _messageSenderOptions.Value.Accounts.TryGetValue(message.AccountId ?? "Default", out var acc)
                ? acc : throw new InvalidOperationException($"Account with ID '{message.AccountId}' was not found.");

            var result = new MessageResult();
            result.Recipients = message.Recipients.ToDictionary(recipient => recipient, _ => false);

            foreach (var recipient in message.Recipients)
            {
                try
                {
                    var response = await MessageResource.CreateAsync(
                          body: message.Body,
                          from: new PhoneNumber(account.PhoneNumber),
                          to: new PhoneNumber(recipient));

                    result.Recipients[recipient] = true;
                }
                catch (Exception ex)
                {
                    _messageSenderLogger.LogError(ex, "Failed to send SMS to '{Recipient}'.", recipient);
                }
            }

            return result;
        }
    }
}