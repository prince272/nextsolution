using System.Reflection;

namespace NextSolution._1.Server.Helpers
{
    public static class AssemblyHelper
    {
        public static IEnumerable<Assembly> GetAssemblies(Func<Assembly, bool>? predicate = null)
        {
            var returnAssemblies = new List<Assembly>();
            var loadedAssemblies = new HashSet<string>();
            var assembliesToCheck = new Queue<Assembly>();

            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                loadedAssemblies.Add(entryAssembly.GetName().FullName);
                returnAssemblies.Add(entryAssembly);
                assembliesToCheck.Enqueue(entryAssembly);
            }

            // Add all currently loaded assemblies to loadedAssemblies HashSet
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!loadedAssemblies.Contains(assembly.GetName().FullName, StringComparer.OrdinalIgnoreCase))
                {
                    loadedAssemblies.Add(assembly.GetName().FullName);
                    returnAssemblies.Add(assembly);
                    assembliesToCheck.Enqueue(assembly);
                }
            }

            while (assembliesToCheck.Count > 0)
            {
                var assemblyToCheck = assembliesToCheck.Dequeue();

                foreach (var reference in assemblyToCheck.GetReferencedAssemblies())
                {
                    if (!loadedAssemblies.Contains(reference.FullName, StringComparer.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var referencedAssembly = Assembly.Load(reference);
                            assembliesToCheck.Enqueue(referencedAssembly);
                            loadedAssemblies.Add(reference.FullName);
                            returnAssemblies.Add(referencedAssembly);
                        }
                        catch
                        {
                            // Add the reference to loadedAssemblies to avoid an infinite loop
                            loadedAssemblies.Add(reference.FullName);
                        }
                    }
                }
            }

            return predicate != null ? returnAssemblies.Where(predicate) : (IEnumerable<Assembly>)returnAssemblies;
        }

        public static IEnumerable<Assembly> GetAppAssemblies()
        {
            return GetAssemblies(x => x.FullName?.StartsWith("NextSolution._1") ?? false);
        }

        public static IEnumerable<Assembly> GetRclAssemblies()
        {
            var rclAssemblies = new List<Assembly>();
            var allAssemblies = GetAssemblies();

            foreach (var assembly in allAssemblies)
            {
                var hasAnyMvcReference = assembly.GetReferencedAssemblies().Select(x => x.Name).Intersect(RclReferences).Any();
                if (hasAnyMvcReference)
                {
                    rclAssemblies.Add(assembly);
                }
            }

            return rclAssemblies;
        }

        /// <summary>
        /// If any of the following references are added to library, then the library is said to RCL library
        /// </summary>
        private static readonly List<string> RclReferences = new()
        {
            "Microsoft.AspNetCore.Mvc",
            "Microsoft.AspNetCore.Mvc.Abstractions",
            "Microsoft.AspNetCore.Mvc.ApiExplorer",
            "Microsoft.AspNetCore.Mvc.Core",
            "Microsoft.AspNetCore.Mvc.Cors",
            "Microsoft.AspNetCore.Mvc.DataAnnotations",
            "Microsoft.AspNetCore.Mvc.Formatters.Json",
            "Microsoft.AspNetCore.Mvc.Formatters.Xml",
            "Microsoft.AspNetCore.Mvc.Localization",
            "Microsoft.AspNetCore.Mvc.NewtonsoftJson",
            "Microsoft.AspNetCore.Mvc.Razor",
            "Microsoft.AspNetCore.Mvc.RazorPages",
            "Microsoft.AspNetCore.Mvc.TagHelpers",
            "Microsoft.AspNetCore.Mvc.ViewFeatures"
        };
    }

    public class AssemblyInfo
    {
        public AssemblyInfo(Assembly assembly)
        {
            this.assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        }

        private readonly Assembly assembly;

        /// <summary>
        /// Gets the title property
        /// </summary>
        public string ProductTitle => GetAttributeValue<AssemblyTitleAttribute>(a => a.Title,
                       Path.GetFileNameWithoutExtension(assembly.Location)) ?? string.Empty;

        /// <summary>
        /// Gets the application's version
        /// </summary>
        public string Version
        {
            get
            {
                var version = assembly.GetName()?.Version;
                if (version != null)
                    return version.ToString();
                else
                    return "1.0.0.0";
            }
        }

        /// <summary>
        /// Gets the description about the application.
        /// </summary>
        public string Description => GetAttributeValue<AssemblyDescriptionAttribute>(a => a.Description) ?? string.Empty;


        /// <summary>
        ///  Gets the product's full name.
        /// </summary>
        public string Product => GetAttributeValue<AssemblyProductAttribute>(a => a.Product) ?? string.Empty;

        /// <summary>
        /// Gets the copyright information for the product.
        /// </summary>
        public string Copyright => GetAttributeValue<AssemblyCopyrightAttribute>(a => a.Copyright) ?? string.Empty;

        /// <summary>
        /// Gets the company information for the product.
        /// </summary>
        public string Company => GetAttributeValue<AssemblyCompanyAttribute>(a => a.Company) ?? string.Empty;

        protected string? GetAttributeValue<TAttr>(Func<TAttr,
          string> resolveFunc, string? defaultResult = null) where TAttr : Attribute
        {
            object[] attributes = assembly.GetCustomAttributes(typeof(TAttr), false);
            if (attributes.Length > 0)
                return resolveFunc((TAttr)attributes[0]);
            else
                return defaultResult;
        }
    }
}
