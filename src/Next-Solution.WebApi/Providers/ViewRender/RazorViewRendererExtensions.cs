using Next_Solution.WebApi.Helpers;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.ObjectPool;
using System.Diagnostics;
using System.Reflection;

namespace Next_Solution.WebApi.Providers.ViewRender
{
    public static class RazorViewRendererExtensions
    {
        public static IServiceCollection AddRazorViewRenderer(this IServiceCollection services, IEnumerable<Assembly> assemblies, Action<RazorViewRendererOptions> options)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (assemblies == null) throw new ArgumentNullException(nameof(assemblies));

            services.Configure(options);
            services.AddRazorViewRenderer(assemblies);
            return services;
        }

        public static IServiceCollection AddRazorViewRenderer(this IServiceCollection services, Assembly assembly, Action<RazorViewRendererOptions> options)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            return services.AddRazorViewRenderer(new[] { assembly }, options);
        }

        public static IServiceCollection AddRazorViewRenderer(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (assemblies == null) throw new ArgumentNullException(nameof(assemblies));

            var mainAssembly = typeof(RazorViewRenderer).Assembly;
            var mainAssemblyName = mainAssembly.GetName().Name!;
            var assembliesBaseDirectory = AppContext.BaseDirectory;

            var webRootName = "wwwroot";
            var webRootDirectory = Directory.Exists(Path.Combine(assembliesBaseDirectory, webRootName)) ? Path.Combine(assembliesBaseDirectory, webRootName) : assembliesBaseDirectory;

            var fileProvider = new PhysicalFileProvider(assembliesBaseDirectory);

            if (!services.Any(_ => _.ServiceType == typeof(IWebHostEnvironment)))
            {
                services.TryAddSingleton<IWebHostEnvironment>(new RazorViewHostingEnvironment
                {
                    ApplicationName = mainAssemblyName,
                    ContentRootPath = assembliesBaseDirectory,
                    ContentRootFileProvider = fileProvider,
                    WebRootPath = webRootDirectory,
                    WebRootFileProvider = new PhysicalFileProvider(webRootDirectory)
                });
            }

            services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            services.TryAddSingleton<DiagnosticSource>(new DiagnosticListener(mainAssemblyName));
            services.TryAddSingleton(new DiagnosticListener(mainAssemblyName));
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.TryAddSingleton<ConsolidatedAssemblyApplicationPartFactory>();
            services.AddHttpContextAccessor();
            var builder = services.AddMvcCore().AddRazorViewEngine();

            builder.ConfigureApplicationPartManager(manager =>
            {
                assemblies ??= new Assembly[0];
                assemblies = assemblies.Concat(new[] { mainAssembly }).ToArray();

                foreach (var assembly in assemblies)
                {
                    var applicationPartFactory = ApplicationPartFactory.GetApplicationPartFactory(assembly);
                    var applicationParts = applicationPartFactory.GetApplicationParts(assembly);

                    foreach (var applicationPart in applicationParts)
                    {
                        var applicationPartExists = manager.ApplicationParts.Any(x => x.GetType() == applicationPart.GetType() && x.Name == applicationPart.Name);

                        if (!applicationPartExists)
                        {
                            manager.ApplicationParts.Add(applicationPart);
                        }
                    }
                }
            });

            services.TryAddTransient<IViewRenderer, RazorViewRenderer>();
            return services;
        }

        public static IServiceCollection AddRazorViewRenderer(this IServiceCollection services, Assembly assembly)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            return services.AddRazorViewRenderer(new[] { assembly });
        }
    }

    internal class RazorViewHostingEnvironment : IWebHostEnvironment
    {
        public RazorViewHostingEnvironment()
        {
        }

        public string EnvironmentName { get; set; } = null!;
        public string ApplicationName { get; set; } = null!;
        public string WebRootPath { get; set; } = null!;
        public IFileProvider WebRootFileProvider { get; set; } = null!;
        public string ContentRootPath { get; set; } = null!;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
