using Microsoft.Extensions.DependencyInjection.Extensions;
using NextSolution.WebApi.Providers.Messaging;

namespace NextSolution.WebApi.Providers.Messaging.MailKit
{
    public static class MailKitMessageSenderExtensions
    {
        public static IServiceCollection AddMailKitMessageSender(this IServiceCollection services, Action<MailKitMessageSenderOptions> options)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (options == null) throw new ArgumentNullException(nameof(options));
            services.Configure(options);
            services.AddMailKitMessageSender();
            return services;
        }

        public static IServiceCollection AddMailKitMessageSender(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.TryAddSingleton<IMessageSender, MessageSender>();
            services.AddScoped<IMessageHandler, MailKitMessageHandler>();
            return services;
        }
    }
}
