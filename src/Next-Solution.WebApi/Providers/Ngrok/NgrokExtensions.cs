using System.Net.Http.Headers;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Next_Solution.WebApi.Providers.Ngrok
{
    public static class NgrokExtensions
    {
        public static void AddNgrokLifetimeHook(
            this IServiceCollection services,
            INgrokLifetimeHook hook)
        {
            services.AddSingleton(hook);
        }

        public static void AddNgrokLifetimeHook<THook>(
            this IServiceCollection services) where THook : class, INgrokLifetimeHook
        {
            services.AddSingleton<INgrokLifetimeHook, THook>();
        }

        public static void AddNgrok(this IServiceCollection services, IConfiguration configuration)
        {
            AddNgrokInternal(services);
            services.Configure<NgrokOptions>(configuration);
        }

        public static void AddNgrok(this IServiceCollection services, Action<NgrokOptions> configureOptions)
        {
            var optionsBuilder = AddNgrokInternal(services);
            optionsBuilder.Configure(configureOptions);
        }

        public static void AddNgrok(this IServiceCollection services)
        {
            AddNgrokInternal(services);
        }

        private static void AddHostedServiceInternal(IServiceCollection services)
        {
            services.AddSingleton<INgrokHostedService, NgrokHostedService>();
            services.AddHostedService(x => x.GetRequiredService<INgrokHostedService>());
        }

        public static void AddNgrokHostedService(this IServiceCollection services, IConfiguration configuration)
        {
            AddNgrok(services, configuration);
            AddHostedServiceInternal(services);
        }

        public static void AddNgrokHostedService(this IServiceCollection services, Action<NgrokOptions> configureOptions)
        {
            AddNgrok(services, configureOptions);
            AddHostedServiceInternal(services);
        }

        public static void AddNgrokHostedService(this IServiceCollection services)
        {
            AddNgrok(services);
            AddHostedServiceInternal(services);
        }

        private static OptionsBuilder<NgrokOptions> AddNgrokInternal(IServiceCollection services)
        {
            services.AddLogging();

            services.AddTransient<INgrokDownloader, NgrokDownloader>();
            services.AddSingleton<INgrokApiClient, NgrokApiClient>();

            var optionsBuilder = services.AddOptions<NgrokOptions>();

            services.AddSingleton<INgrokProcess, NgrokProcess>();
            services.AddSingleton<INgrokService, NgrokService>();

            services.AddSingleton<IFlurlClientCache>(sp => new FlurlClientCache());

            services.AddHttpClient<INgrokDownloader, NgrokDownloader>(httpClient =>
            {
                httpClient.BaseAddress = new Uri("https://bin.equinox.io");
            });

            return optionsBuilder;
        }
    }
}