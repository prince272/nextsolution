using Microsoft.Extensions.DependencyInjection.Extensions;

namespace NextSolution.Server.Providers.Messaging.Arkesel
{
    public static class ArkeselMessageSenderExtensions
    {
        public static IServiceCollection AddArkeselMessageSender(this IServiceCollection services, Action<ArkeselMessageSenderOptions> options)
        {
            services.Configure(options);
            services.AddArkeselMessageSender();
            return services;
        }

        public static IServiceCollection AddArkeselMessageSender(this IServiceCollection services)
        {
            services.TryAddSingleton<IMessageSender, MessageSender>();
            services.AddKeyedScoped<IMessageHandler, ArkeselMessageHandler>(nameof(MessageSender));
            return services;
        }
    }
}
