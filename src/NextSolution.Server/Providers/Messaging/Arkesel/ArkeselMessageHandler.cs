

using Flurl.Http;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace NextSolution.Server.Providers.Messaging.Arkesel
{
    public class ArkeselMessageHandler : IMessageHandler
    {
        private readonly IFlurlClient _arkeselClient;
        private readonly IOptions<ArkeselMessageSenderOptions> _messageSenderOptions;
        private readonly ILogger<ArkeselMessageHandler> _logger;

        public ArkeselMessageHandler(IOptions<ArkeselMessageSenderOptions> messageSenderOptions, ILogger<ArkeselMessageHandler> logger)
        {
            _messageSenderOptions = messageSenderOptions ?? throw new ArgumentNullException(nameof(messageSenderOptions));
            _arkeselClient = new FlurlClient(messageSenderOptions.Value.ApiUrl).WithHeader("api-key", _messageSenderOptions.Value.ApiKey);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public MessageChannel Channels => MessageChannel.Sms;

        public async Task SenderAsync(MessageChannel channel, Message message, CancellationToken cancellationToken = default)
        {
            if (channel == MessageChannel.Sms)
            {
                // Build request
                var request = _arkeselClient.Request("/api/v2/sms/send").AllowAnyHttpStatus();
                var requestPayload = new
                {
                    sender = message.SenderId ?? _messageSenderOptions.Value.SenderId,
                    message = message.Body,
                    recipients = message.Recipients,
                };

                // Log request
                _logger.LogInformation("[{MethodName}] => [POST] {RequestUrl}, Payload: {Payload}",
                    nameof(SenderAsync), request.Url, JsonSerializer.Serialize(requestPayload));

                var response = await request.PostJsonAsync(requestPayload, cancellationToken: cancellationToken);
                var responseContent = await response.ResponseMessage.Content.ReadAsStringAsync(cancellationToken);

                // Log response
                if (!response.ResponseMessage.IsSuccessStatusCode)
                {
                    _logger.LogError("[{MethodName}] => [POST] {RequestUrl}, Payload: {Payload}, [{ResponseStatus}] Response: {Response}",
                        nameof(SenderAsync), request.Url, JsonSerializer.Serialize(requestPayload), response.ResponseMessage.StatusCode, responseContent);

                    // throw exception with response content, add message
                    throw new HttpRequestException($"Failed to send message.");
                }

                _logger.LogInformation("[{MethodName}] => [POST] {RequestUrl}, Payload: {Payload}, [{ResponseStatus}] Response: {Response}",
                    nameof(SenderAsync), request.Url, JsonSerializer.Serialize(requestPayload), response.ResponseMessage.StatusCode, responseContent);
            }
        }
    }
}