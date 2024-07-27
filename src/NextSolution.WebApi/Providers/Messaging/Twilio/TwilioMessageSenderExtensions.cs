using Microsoft.Extensions.DependencyInjection.Extensions;
using NextSolution.WebApi.Providers.Messaging;

namespace NextSolution.WebApi.Providers.Messaging.Twilio
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
