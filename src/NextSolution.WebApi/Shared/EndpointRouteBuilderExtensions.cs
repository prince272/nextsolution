using NextSolution.Core.Helpers;

namespace NextSolution.WebApi.Shared
{
    public static class EndpointRouteBuilderExtensions
    {
        public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var endpointTypes = TypeHelper.GetTypesFromApplicationDependencies().Where(type => type.IsClass && !type.IsAbstract && !type.IsGenericType && type.IsCompatibleWith(typeof(IEndpoints)));

            foreach (var concreteType in endpointTypes)
            {
                if (Activator.CreateInstance(concreteType) is IEndpoints instance)
                {
                    instance.Map(endpoints);
                }
            }

            return endpoints;
        }
    }
}
