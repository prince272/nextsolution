using Microsoft.Extensions.DependencyInjection.Extensions;

namespace NextSolution._1.Server.Providers.Messaging.MailKit
{
    public static class TwilioMessageSenderExtensions
    {
        public static IServiceCollection AddTwilioMessageSender(this IServiceCollection services, Action<TwilioMessageSenderOptions> options)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (options == null) throw new ArgumentNullException(nameof(options));
            services.Configure(options);
            services.AddTwilioMessageSender();
            return services;
        }

        public static IServiceCollection AddTwilioMessageSender(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.TryAddSingleton<IMessageSender, MessageSender>();
            services.AddScoped<IMessageHandler, TwilioMessageHandler>();
            return services;
        }
    }
}
