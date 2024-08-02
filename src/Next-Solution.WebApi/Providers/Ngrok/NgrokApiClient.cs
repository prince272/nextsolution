using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Next_Solution.WebApi.Providers.Ngrok.Models;

namespace Next_Solution.WebApi.Providers.Ngrok
{
    public class NgrokApiClient : INgrokApiClient
    {
        private readonly IFlurlClient _client;
        private readonly ILogger<NgrokApiClient> _logger;

        public NgrokApiClient(
            IFlurlClientCache clientCache,
            ILogger<NgrokApiClient> logger)
        {
            _client = clientCache.GetOrAdd("NgrokApi", "http://localhost:4040/api/", x =>
            {
                x.Settings.JsonSerializer = new DefaultJsonSerializer(new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            });
            _logger = logger;
        }

        public async Task<bool> IsNgrokReady(CancellationToken cancellationToken)
        {
            try
            {
                await CreateRequest("tunnels").GetAsync(HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                //ignore exceptions.
                _logger.LogTrace(ex, "Ngrok is not ready yet.");
                return false;
            }
        }

        public async Task<TunnelResponse[]> GetTunnelsAsync(CancellationToken cancellationToken)
        {
            try
            {
                var tunnels = await CreateRequest("tunnels")
                    .GetJsonAsync<TunnelListResponse>(
                        HttpCompletionOption.ResponseContentRead,
                        cancellationToken);

                var tunnelResponses = tunnels.Tunnels.ToArray();
                _logger.LogTrace("Tunnels: {@Tunnels}", new object[] { tunnelResponses });

                return tunnelResponses;
            }
            catch (FlurlHttpException ex)
            {
                _logger.LogError(ex, "Could not list tunnels.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occured during tunnel fetching.");
                throw;
            }
        }

        public async Task<TunnelResponse> CreateTunnelAsync(
            string projectName,
            Uri address,
            string? domain,
            CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var request = new CreateTunnelApiRequest()
                {
                    Name = projectName,
                    Address = address.Port.ToString(CultureInfo.InvariantCulture),
                    Protocol = address.Scheme,
                    Domain = domain,
                };

                _logger.LogInformation("Creating tunnel {TunnelName}", projectName);

                try
                {
                    _logger.LogTrace("Sending request {@TunnelRequest}", request);

                    var response = await CreateRequest("tunnels")
                        .PostJsonAsync(
                            request,
                            HttpCompletionOption.ResponseContentRead,
                            cancellationToken)
                        .ReceiveJson<TunnelResponse>();
                    _logger.LogInformation("Tunnel {@Tunnel} created", response);

                    return response;
                }
                catch (FlurlHttpException ex)
                {
                    var error = await ex.Call.Response.GetJsonAsync<ErrorResponse>();

                    var isNotReadyToStartTunnels = error.ErrorCode == 104;
                    if (!isNotReadyToStartTunnels)
                    {
                        _logger.LogError(ex, "Tunnel creation failed: {@Error}", error);
                        throw;
                    }

                    _logger.LogTrace("Tunnel creation failed due to Ngrok not being ready - will retry.");
                    await Task.Delay(100, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unhandled exception occured during tunnel creation.");
                    throw;
                }
            }

            throw new OperationCanceledException();
        }

        private IFlurlRequest CreateRequest(params object[] pathSegments)
        {
            var request = _client.Request(pathSegments);
            return request;
        }
    }
}