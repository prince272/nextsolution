using Microsoft.Extensions.Options;
using Next_Solution.WebApi.Providers.Ngrok.Models;

namespace Next_Solution.WebApi.Providers.Ngrok
{
    public class NgrokService : INgrokService
    {
        private readonly INgrokDownloader _downloader;
        private readonly INgrokProcess _process;
        private readonly IOptionsMonitor<NgrokOptions> _options;
        private readonly IEnumerable<INgrokLifetimeHook> _hooks;
        private readonly INgrokApiClient _ngrok;
        private readonly ILogger<NgrokService> _logger;

        private bool _isInitialized;

        private readonly HashSet<TunnelResponse> _activeTunnels;

        public IReadOnlyCollection<TunnelResponse> ActiveTunnels => _activeTunnels;

        public NgrokService(
            INgrokDownloader downloader,
            INgrokProcess process,
            IOptionsMonitor<NgrokOptions> options,
            IEnumerable<INgrokLifetimeHook> hooks,
            INgrokApiClient ngrok,
            ILogger<NgrokService> logger)
        {
            _downloader = downloader;
            _process = process;
            _options = options;
            _hooks = hooks;
            _ngrok = ngrok;
            _logger = logger;

            _activeTunnels = new HashSet<TunnelResponse>();
        }

        public async Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
        {
            while (!ActiveTunnels.Any() && !cancellationToken.IsCancellationRequested)
                await Task.Delay(25, cancellationToken);
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (_isInitialized)
                return;

            _isInitialized = true;

            await _downloader.DownloadExecutableAsync(cancellationToken);
            await _process.StartAsync();
        }

        public async Task<bool> TryInitializeAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await InitializeAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "An error occurred while initializing the ngrok tunnel.");
                return false;
            }
        }

        public async Task<TunnelResponse> StartAsync(
            Uri host,
            CancellationToken cancellationToken)
        {
            var tunnel = await GetOrCreateTunnelAsync(host, _options.CurrentValue.Domain, cancellationToken);
            _activeTunnels.Clear();
            _activeTunnels.Add(tunnel);

            await Task.WhenAll(_hooks
                .ToArray()
                .Select(async hook =>
                {
                    try
                    {
                        await hook.OnCreatedAsync(tunnel, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ngrok cook OnCreatedAsync failed.");
                    }
                }));

            return tunnel;
        }

        private async Task<TunnelResponse> GetOrCreateTunnelAsync(Uri host, string? domain, CancellationToken cancellationToken)
        {
            var existingTunnels = await _ngrok.GetTunnelsAsync(cancellationToken);
            var existingTunnel = existingTunnels.FirstOrDefault(x => x.Name == AppDomain.CurrentDomain.FriendlyName);
            if (existingTunnel != null)
                return existingTunnel;

            return await _ngrok.CreateTunnelAsync(
                AppDomain.CurrentDomain.FriendlyName,
                host,
                domain,
                cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            var hooks = _hooks.ToArray();
            var activeTunnels = _activeTunnels.ToArray();

            _activeTunnels.Clear();
            await _process.StopAsync();

            await Task.WhenAll(activeTunnels
                .Select(tunnel => Task.WhenAll(hooks
                    .Select(async hook =>
                    {
                        try
                        {
                            await hook.OnDestroyedAsync(tunnel, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Ngrok cook OnDestroyedAsync failed.");
                        }
                    }))));
        }
    }
}