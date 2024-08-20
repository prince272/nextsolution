namespace Next_Solution.WebApi.Providers.Ngrok
{
    public interface INgrokDownloader
    {
        Task DownloadExecutableAsync(CancellationToken cancellationToken);
    }
}