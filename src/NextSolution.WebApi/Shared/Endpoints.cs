using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace NextSolution.WebApi.Shared
{
    public abstract class Endpoints
    {
        private readonly IEndpointRouteBuilder _endpointRouteBuilder;

        public Endpoints(IEndpointRouteBuilder endpointRouteBuilder)
        {
            _endpointRouteBuilder = endpointRouteBuilder ?? throw new Exception(nameof(endpointRouteBuilder));
        }

        protected virtual RouteGroupBuilder MapGroup(string prefix)
        {
            var groupName = GetType().Name;

            return _endpointRouteBuilder
                .MapGroup(prefix)
                //.WithGroupName(groupName)
                .WithTags(groupName)
                .WithOpenApi();
        }

        public abstract void Configure();
    }
}