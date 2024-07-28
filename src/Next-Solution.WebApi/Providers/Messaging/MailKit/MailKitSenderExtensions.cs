using Microsoft.Extensions.DependencyInjection.Extensions;
using Next_Solution.WebApi.Providers.Messaging;

namespace Next_Solution.WebApi.Providers.Messaging.MailKit
{
    public static class MailKitSenderExtensions
    {
        public static IServiceCollection AddMailKitSender(this IServiceCollection services, Action<MailKitSenderOptions> options)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (options == null) throw new ArgumentNullException(nameof(options));
            services.Configure(options);
            services.AddMailKitSender();
            return services;
        }

        public static IServiceCollection AddMailKitSender(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.TryAddSingleton<IMessageSender, MessageSender>();
            services.AddScoped<IMessageHandler, MailKitHandler>();
            return services;
        }
    }
}
