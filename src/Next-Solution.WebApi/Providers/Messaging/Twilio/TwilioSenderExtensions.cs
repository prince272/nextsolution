using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Next_Solution.WebApi.Providers.Messaging.Twilio
{
    public static class TwilioSenderExtensions
    {
        public static IServiceCollection AddTwilioSender(this IServiceCollection services, Action<TwilioSenderOptions> options)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (options == null) throw new ArgumentNullException(nameof(options));
            services.Configure(options);
            services.AddTwilioSender();
            return services;
        }

        public static IServiceCollection AddTwilioSender(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.TryAddSingleton<IMessageSender, MessageSender>();
            services.AddScoped<IMessageHandler, TwilioHandler>();
            return services;
        }
    }
}
