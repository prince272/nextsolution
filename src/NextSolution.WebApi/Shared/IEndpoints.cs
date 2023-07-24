namespace NextSolution.WebApi.Shared
{
    public interface IEndpoints
    {
        string Name { get; }

        void Map(IEndpointRouteBuilder endpoints);
    }
}