namespace NextSolution.Server.Providers.Messaging
{
    public class MessageSender : IMessageSender
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public MessageSender(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        public Task SenderAsync(MessageChannel channel, Message message, CancellationToken cancellationToken = default)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            using var serviceScope = _serviceScopeFactory.CreateScope();
            var serviceProvider = serviceScope.ServiceProvider;
            var messageHandlers = serviceProvider.GetKeyedServices<IMessageHandler>(nameof(MessageSender));
            var messageHandler = messageHandlers.FirstOrDefault(h => h.Channels.HasFlag(channel));
            if (messageHandler == null) throw new InvalidOperationException($"No message handler found for channel '{channel}'.");
            return messageHandler.SenderAsync(channel, message);
        }
    }

    public interface IMessageSender
    {
        Task SenderAsync(MessageChannel channel, Message message, CancellationToken cancellationToken = default);
    }

    public interface IMessageHandler
    {
        MessageChannel Channels { get; }

        Task SenderAsync(MessageChannel channel, Message message, CancellationToken cancellationToken = default);
    }
}
