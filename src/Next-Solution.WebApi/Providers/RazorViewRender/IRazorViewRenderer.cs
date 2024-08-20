namespace Next_Solution.WebApi.Providers.RazorViewRender
{
    public interface IRazorViewRenderer
    {
        Task<string> RenderAsync(string name, object? model = null, CancellationToken cancellationToken = default);
    }
}