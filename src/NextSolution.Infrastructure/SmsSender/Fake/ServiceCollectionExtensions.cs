using Microsoft.Extensions.DependencyInjection;
using NextSolution.Core.Extensions.SmsSender;

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
