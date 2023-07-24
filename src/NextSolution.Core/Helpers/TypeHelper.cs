using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Helpers
{
    public static class TypeHelper
    {
        public static IEnumerable<Type> GetTypesFromApplicationDependencies()
        {
            return GetTypesFromApplicationDependencies(_ => true);
        }

        public static IEnumerable<Type> GetTypesFromApplicationDependencies(Func<Assembly, bool> predicate)
        {
            try
            {
                return GetTypesFromDependencyContext(DependencyContext.Default!, predicate);
            }
            catch
            {
                // Something went wrong when loading the DependencyContext, fall
                // back to loading all referenced assemblies of the entry assembly...
                return GetTypesFromAssemblyDependencies(Assembly.GetEntryAssembly()
                                             ?? throw new InvalidOperationException("Could not get entry assembly."));
            }
        }

        public static IEnumerable<Type> GetTypesFromDependencyContext(DependencyContext context)
        {
            return GetTypesFromDependencyContext(context, _ => true);
        }

        public static IEnumerable<Type> GetTypesFromDependencyContext(DependencyContext context, Func<Assembly, bool> predicate)
        {
            var assemblyNames = context.RuntimeLibraries
                .SelectMany(library => library.GetDefaultAssemblyNames(context));

            var assemblies = LoadAssemblies(assemblyNames);

            return GetTypesFromAssemblies(assemblies.Where(predicate));
        }


        public static IEnumerable<Type> GetTypesFromAssemblyDependencies(Assembly assembly)
        {
            var assemblies = new List<Assembly> { assembly };

            try
            {
                var dependencyNames = assembly.GetReferencedAssemblies();

                assemblies.AddRange(LoadAssemblies(dependencyNames));

                return GetTypesFromAssemblies(assemblies);
            }
            catch
            {
                return GetTypesFromAssemblies(assemblies);
            }
        }

        public static IEnumerable<Type> GetTypesFromAssembliesOf(IEnumerable<Type> types)
        {
            return GetTypesFromAssemblies(types.Select(t => t.Assembly));
        }

        public static IEnumerable<Type> GetTypesFromAssemblyOf<T>()
        {
            return GetTypesFromAssembliesOf(new[] { typeof(T) });
        }

        public static IEnumerable<Type> GetTypesFromAssemblies(params Assembly[] assemblies)
        {
            return GetTypesFromAssemblies(assemblies.AsEnumerable());
        }


        public   static IEnumerable<Type> GetTypesFromAssemblies(IEnumerable<Assembly> assemblies)
        {
            return assemblies.SelectMany(asm => asm.DefinedTypes).Select(x => x.AsType()).ToArray();
        }

        private static IEnumerable<Assembly> LoadAssemblies(IEnumerable<AssemblyName> assemblyNames)
        {
            var assemblies = new List<Assembly>();

            foreach (var assemblyName in assemblyNames)
            {
                try
                {
                    // Try to load the referenced assembly...
                    assemblies.Add(Assembly.Load(assemblyName));
                }
                catch
                {
                    // Failed to load assembly. Skip it.
                }
            }

            return assemblies;
        }
    }
}
