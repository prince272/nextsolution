namespace Next_Solution.WebApi.Providers.ViewRender
{
    public interface IViewRenderer
    {
        Task<string> RenderAsync(string name, object? model = null, CancellationToken cancellationToken = default);
    }
}