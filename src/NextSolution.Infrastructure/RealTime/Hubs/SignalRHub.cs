using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NextSolution.Core.Extensions.Identity;
using NextSolution.Core.Extensions.RealTime;
using NextSolution.Infrastructure.Identity;
using NextSolution.Infrastructure.RealTime;

namespace NextSolution.Infrastructure.RealTime.Hubs
{
    public class SignalRHub : Hub
    {
        private readonly IOnlineClientManager _onlineClientManager;
        private readonly ILogger<SignalRHub> _logger;
        private readonly IUserContext _userContext;

        public SignalRHub(IOnlineClientManager onlineClientManager, ILogger<SignalRHub> logger, IUserContext userContext)
        {
            _onlineClientManager = onlineClientManager ?? throw new ArgumentNullException(nameof(onlineClientManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        public override Task OnConnectedAsync()
        {
            var client = CreateClientForCurrentConnection();

            _logger.LogDebug("A client is connected: " + client);

            _onlineClientManager.Add(client);
            return Task.CompletedTask;
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogDebug("A client is disconnected: " + Context.ConnectionId);

            try
            {
                _onlineClientManager.Remove(Context.ConnectionId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.ToString(), ex);
            }

            return Task.CompletedTask;
        }

        protected virtual IOnlineClient CreateClientForCurrentConnection()
        {
            return new OnlineClient(Context.ConnectionId, _userContext.IpAddress, _userContext.DeviceId, _userContext.UserId, _userContext.UserAgent);
        }
    }
}