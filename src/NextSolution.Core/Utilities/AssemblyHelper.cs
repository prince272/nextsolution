using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Utilities
{
    public static class AssemblyHelper
    {
        public static IEnumerable<Assembly> GetAssemblies()
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

            return returnAssemblies;
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
}