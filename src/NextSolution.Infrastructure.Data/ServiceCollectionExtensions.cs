using Microsoft.Extensions.DependencyInjection;
using NextSolution.Core.Shared;
using NextSolution.Core.Utilities;
using System.Reflection;

namespace NextSolution.Infrastructure.Data
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            var repositoryTypes = assemblies.SelectMany(_ => _.DefinedTypes).Select(_ => _.AsType())
                .Where(type => type.IsClass && !type.IsAbstract && type.IsCompatibleWith(typeof(IRepository<>)));

            foreach (var concreteType in repositoryTypes)
            {
                var matchingInterfaceType = concreteType.GetInterfaces().FirstOrDefault(x => string.Equals(x.Name, $"I{concreteType.Name}", StringComparison.Ordinal));

                if (matchingInterfaceType != null)
                {
                    services.AddScoped(matchingInterfaceType, concreteType);
                }
            }

            return services;
        }
    }
}
