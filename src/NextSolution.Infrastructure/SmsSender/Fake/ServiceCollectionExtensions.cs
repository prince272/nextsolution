using Microsoft.Extensions.DependencyInjection;
using NextSolution.Core.Extensions.SmsSender;
using NextSolution.Infrastructure.EmailSender.MailKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Infrastructure.SmsSender.Fake
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFakeSmsSender(this IServiceCollection services)
        {
            services.AddTransient<ISmsSender, FakeSmsSender>();
            return services;
        }
    }
}
