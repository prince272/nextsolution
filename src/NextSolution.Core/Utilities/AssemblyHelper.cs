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

    }
}