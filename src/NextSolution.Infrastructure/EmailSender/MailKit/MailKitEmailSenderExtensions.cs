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

namespace NextSolution.Infrastructure.EmailSender.MailKit
{
    public static class MailKitEmailSenderExtensions
    {
        public static IServiceCollection AddMailKitEmailSender(this IServiceCollection services, Action<MailKitEmailOptions>? options = null)
        {
            if (options != null) services.Configure(options);
            services.AddTransient<IEmailSender, MailKitEmailSender>();
            return services;
        }
    }
}
