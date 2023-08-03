using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using NextSolution.Core.Extensions.ViewRenderer;
using NextSolution.Core.Utilities;
using NextSolution.Infrastructure.EmailSender.MailKit;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace NextSolution.Infrastructure.ViewRenderer.Razor
{
    public static class RazorViewRendererExtensions
    {
        public static IServiceCollection AddRazorViewRenderer(this IServiceCollection services, Action<RazorViewRendererOptions> options)
        {
            services.Configure(options);
            services.AddRazorViewRenderer();
            return services;
        }

        public static IServiceCollection AddRazorViewRenderer(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RazorViewRendererOptions>(configuration);
            services.AddRazorViewRenderer();
            return services;
        }

        public static IServiceCollection AddRazorViewRenderer(this IServiceCollection services)
        {

            if (services == null) throw new ArgumentNullException(nameof(services));

            var identifier = typeof(RazorViewRenderer).Assembly.GetName().Name!;

            // source: https://docs.microsoft.com/en-us/dotnet/core/deploying/single-file#api-incompatibility
            var assembliesBaseDirectory = AppContext.BaseDirectory;

            var mainExecutableDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName);

            var webRootName = "wwwroot";
            var webRootDirectory = Directory.Exists(Path.Combine(assembliesBaseDirectory, webRootName)) ? Path.Combine(assembliesBaseDirectory, webRootName) : assembliesBaseDirectory;

            Debug.WriteLine($"Assemblies Base Directory: {assembliesBaseDirectory}");
            Debug.WriteLine($"Main Executable Directory: {mainExecutableDirectory}");
            Debug.WriteLine($"Web Root Directory: {webRootDirectory}");

            var fileProvider = new PhysicalFileProvider(assembliesBaseDirectory);

            // MVC, API applications will have this object already.
            if (!services.Any(x => x.ServiceType == typeof(IWebHostEnvironment)))
            {
                services.TryAddSingleton<IWebHostEnvironment>(new RazorViewHostingEnvironment
                {
                    ApplicationName = identifier,
                    ContentRootPath = assembliesBaseDirectory,
                    ContentRootFileProvider = fileProvider,
                    WebRootPath = webRootDirectory,
                    WebRootFileProvider = new PhysicalFileProvider(webRootDirectory)
                });
            }

            services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            services.TryAddSingleton<DiagnosticSource>(new DiagnosticListener(identifier));
            services.TryAddSingleton(new DiagnosticListener(identifier));
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.TryAddSingleton<ConsolidatedAssemblyApplicationPartFactory>();
            services.AddLogging();
            services.AddHttpContextAccessor();
            var builder = services.AddMvcCore().AddRazorViewEngine();

            builder.ConfigureApplicationPartManager(manager =>
            {
                var applicationParts = new List<ApplicationPart>();

                foreach (var rclAssembly in AssemblyHelper.GetRclAssemblies())
                {
                    var applicationPartFactory = ApplicationPartFactory.GetApplicationPartFactory(rclAssembly);
                    applicationParts.AddRange(applicationPartFactory.GetApplicationParts(rclAssembly));
                }

                Debug.WriteLine($"Found {applicationParts.Count} application parts");

                foreach (var applicationPart in applicationParts)
                {
                    // For MVC projects, application parts are already added by the framework
                    if (!manager.ApplicationParts.Any(x => x.GetType() == applicationPart.GetType() && x.Name == applicationPart.Name))
                    {
                        manager.ApplicationParts.Add(applicationPart);
                        Debug.WriteLine($"Application part added ({applicationPart.Name} {applicationPart.GetType().Name})");
                    }
                    else
                    {
                        Debug.WriteLine($"Application part already added {applicationPart.Name} ({applicationPart.GetType().Name})");
                    }
                }
            });

            services.TryAddTransient<IViewRenderer, RazorViewRenderer>();
            return services;
        }
    }

    internal class RazorViewHostingEnvironment : IWebHostEnvironment
    {
        public RazorViewHostingEnvironment()
        {
        }

        public string EnvironmentName { get; set; } = default!;
        public string ApplicationName { get; set; } = default!;
        public string WebRootPath { get; set; } = default!;
        public IFileProvider WebRootFileProvider { get; set; } = default!;
        public string ContentRootPath { get; set; } = default!;
        public IFileProvider ContentRootFileProvider { get; set; } = default!;
    }
}
