using NextSolution.Core.Utilities;

namespace NextSolution.WebApi.Shared
{
    public static class EndpointRouteBuilderExtensions
    {
        public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
        {
            var endpointTypes = TypeHelper.GetTypesFromApplicationDependencies().Where(type => type.IsClass && !type.IsAbstract && !type.IsGenericType && type.IsCompatibleWith(typeof(Endpoints)));

            foreach (var concreteType in endpointTypes)
            {
                if (Activator.CreateInstance(concreteType, endpointRouteBuilder) is Endpoints endpoints)
                {
                    endpoints.Configure();
                }
            }

            return endpointRouteBuilder;
        }
    }
}
