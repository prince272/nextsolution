using Microsoft.Extensions.DependencyInjection.Extensions;

namespace NextSolution.Server.Providers.Messaging.MailKit
{
    public static class MailKitMessageSenderExtensions
    {
        public static IServiceCollection AddMailKitMessageSender(this IServiceCollection services, Action<MailKitMessageSenderOptions> options)
        {
            services.Configure(options);
            services.AddMailKitMessageSender();
            return services;
        }

        public static IServiceCollection AddMailKitMessageSender(this IServiceCollection services)
        {
            services.TryAddSingleton<IMessageSender, MessageSender>();
            services.AddKeyedScoped<IMessageHandler, MailKitMessageHandler>(nameof(MessageSender));
            return services;
        }
    }
}
