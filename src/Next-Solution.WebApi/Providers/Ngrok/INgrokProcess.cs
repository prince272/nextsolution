namespace Next_Solution.WebApi.Providers.Ngrok
{
    public interface INgrokProcess
    {
        Task StartAsync();
        Task StopAsync();
    }
}