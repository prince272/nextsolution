using Microsoft.Extensions.DependencyInjection;
using NextSolution.Core.Extensions.EmailSender;

namespace NextSolution.Infrastructure.EmailSender.MailKit
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMailKitEmailSender(this IServiceCollection services, Action<MailKitEmailOptions> options)
        {
            services.Configure(options);
            services.AddMailKitEmailSender();
            return services;
        }

        public static IServiceCollection AddMailKitEmailSender(this IServiceCollection services)
        {
            services.AddTransient<IEmailSender, MailKitEmailSender>();
            return services;
        }
    }
}
