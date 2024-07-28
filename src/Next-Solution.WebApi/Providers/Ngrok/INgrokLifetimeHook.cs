using Next_Solution.WebApi.Providers.Ngrok.Models;

namespace Next_Solution.WebApi.Providers.Ngrok;

public interface INgrokLifetimeHook
{
    Task OnCreatedAsync(TunnelResponse tunnel, CancellationToken cancellationToken);
    Task OnDestroyedAsync(TunnelResponse tunnel, CancellationToken cancellationToken);
}