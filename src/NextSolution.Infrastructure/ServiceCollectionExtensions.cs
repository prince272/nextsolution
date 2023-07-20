using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NextSolution.Core.Helpers;
using NextSolution.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            var repositoryTypes = TypeHelper.GetTypesFromApplicationDependencies().Where(type => type.IsClass && !type.IsAbstract && type.IsCompatibleWith(typeof(IRepository<>)));

            foreach (var concreteType in repositoryTypes)
            {
                foreach (var interfaceType in concreteType.FindMatchingInterfaces())
                    services.AddScoped(interfaceType, concreteType);
            }

            return services;
        }
    }
}
