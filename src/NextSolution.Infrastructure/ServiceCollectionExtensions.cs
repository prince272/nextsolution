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
using NextSolution.Infrastructure.EmailSender.MailKit;
using NextSolution.Core.Extensions.EmailSender;

namespace NextSolution.Infrastructure.Data
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMailKitEmailSender(this IServiceCollection services, Action<MailKitEmailOptions> options)
        {
            services.Configure(options);
            services.AddTransient<IEmailSender, MailKitEmailSender>();
            return services;
        }
    }
}
