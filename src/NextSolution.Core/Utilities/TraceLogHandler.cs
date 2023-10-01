using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NextSolution.Core.Utilities
{
    public class TraceLogHandler : DelegatingHandler
    {
        private readonly IServiceProvider services;
        private readonly Func<HttpResponseMessage, bool> _shouldLog;

        public TraceLogHandler(IServiceProvider serviceProvider, Func<HttpResponseMessage, bool> shouldLog)
        {
            services = serviceProvider;
            _shouldLog = shouldLog;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            bool logPayloads = true;

            HttpResponseMessage response = null;
            try
            {
                response = await base.SendAsync(request, cancellationToken);

                // We run the ShouldLog function that calculates, based on HttpResponseMessage, if we should log HttpClient request/response.
                logPayloads = logPayloads || _shouldLog(response);
            }
            catch (Exception)
            {
                // We want to log HttpClient request/response when some exception occurs, so we can reproduce what caused it.
                logPayloads = true;
                throw;
            }
            finally
            {
                // Finally, we check if we decided to log HttpClient request/response or not.
                // Only if we want to, we will have some allocations for the logger and try to read headers and contents.
                if (logPayloads)
                {
                    var logger = services.GetRequiredService<ILogger<TraceLogHandler>>();
                    var scope = new Dictionary<string, object>();

                    scope.TryAdd("Service_RequestHeaders", request);
                    if (request?.Content != null)
                    {
                        scope.Add("Service_RequestBody", await request.Content.ReadAsStringAsync());
                    }
                    scope.TryAdd("Service_ResponseHeaders", response);
                    if (response?.Content != null)
                    {
                        scope.Add("Service_ResponseBody", await response.Content.ReadAsStringAsync());
                    }
                    using (logger.BeginScope(scope))
                    {
                        logger.LogInformation("[TRACE] Service Request/Response", scope);
                    }
                }
            }

            return response;
        }
    }
}