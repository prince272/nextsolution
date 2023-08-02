using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using NextSolution.Core.Extensions.ViewRenderer;
using NextSolution.Core.Utilities;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace NextSolution.Infrastructure.ViewRenderer.Razor
{
    public static class RazorViewRendererExtensions
    {
        public static IServiceCollection AddRazorViewRenderer(this IServiceCollection services, Action<RazorViewRendererOptions>? options = null)
        {

            if (services == null) throw new ArgumentNullException(nameof(services));

            var identifier = typeof(RazorViewRenderer).Assembly.GetName().Name!;

            // source: https://docs.microsoft.com/en-us/dotnet/core/deploying/single-file#api-incompatibility
            var assembliesBaseDirectory = AppContext.BaseDirectory;

            //in .net 5, RCL assemblies are located next the main executable even if /p:IncludeAllContentForSelfExtract=true is provided while publishing
            //also when .net core 3.1 project is published using .net 5 sdk, above scenario happens
            //so, additionally look for RCL assemblies at the main executable directory as well
            var mainExecutableDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName);

            //To add support for MVC application
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
            //ref: https://stackoverflow.com/questions/52041011/aspnet-core-2-1-correct-way-to-load-precompiled-views
            //load view assembly application parts to find the view from shared libraries
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

            if (options != null) services.Configure(options);
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
