using Microsoft.Extensions.DependencyInjection;
using NextSolution.Core.Extensions.RealTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Infrastructure.RealTime
{
    public static class OnlineClientExtensions
    {
        public static IServiceCollection AddOnlineClientManager(this IServiceCollection services)
        {
            services.AddSingleton<IOnlineClientManager, OnlineClientManager>();
            services.AddSingleton<IOnlineClientStore, InMemoryOnlineClientStore>();
            return services;
        }
    }
}
