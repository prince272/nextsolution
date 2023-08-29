using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NextSolution.Core.Entities;
using NextSolution.Core.Extensions.Identity;
using NextSolution.Core.Models.Clients;
using NextSolution.Core.Repositories;
using NextSolution.Core.Services;
using NextSolution.Infrastructure.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Infrastructure.RealTime.SignalR
{
    public class SignalRHub : Hub
    {
        private readonly ClientService _clientService;
        private readonly ILogger<SignalRHub> _logger;

        public SignalRHub(ClientService clientService, ILogger<SignalRHub> logger)
        {
            _clientService = clientService ?? throw new ArgumentNullException(nameof(clientService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                await _clientService.ConnectAsync(new ConnectClientForm { ConnectionId = Context.ConnectionId });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.ToString(), ex);
            }
            finally
            {
                _logger.LogDebug($"A client is connected: {Context.ConnectionId}");
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                await _clientService.DisconnectAsync(new DisconnectClientForm { ConnectionId = Context.ConnectionId });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.ToString(), ex);
            }
            finally
            {
                _logger.LogDebug($"A client is disconnected: {Context.ConnectionId}");
            }
        }
    }
}