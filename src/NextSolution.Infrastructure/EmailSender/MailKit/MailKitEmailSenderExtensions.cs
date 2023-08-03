using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NextSolution.Core.Utilities;
using NextSolution.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NextSolution.Core.Extensions.EmailSender;
using Microsoft.Extensions.Configuration;

namespace NextSolution.Infrastructure.EmailSender.MailKit
{
    public static class MailKitEmailSenderExtensions
    {
        public static IServiceCollection AddMailKitEmailSender(this IServiceCollection services, Action<MailKitEmailOptions> options)
        {
            services.Configure(options);
            services.AddMailKitEmailSender();
            return services;
        }

        public static IServiceCollection AddMailKitEmailSender(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MailKitEmailOptions>(configuration);
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
