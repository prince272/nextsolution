using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NextSolution.Core.Entities;
using NextSolution.Core.Events.Clients;
using NextSolution.Core.Events.Users;
using NextSolution.Core.Exceptions;
using NextSolution.Core.Extensions.Identity;
using NextSolution.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NextSolution.Infrastructure.RealTime
{
    public class SignalRHub : Hub
    {
        private readonly ILogger<SignalRHub> _logger;
        private readonly IMediator _mediator;
        private readonly IClientContext _clientContext;
        private readonly IUserRepository _userRepository;
        private readonly IClientRepository _clientRepository;

        public SignalRHub(ILogger<SignalRHub> logger, IMediator mediator, IClientContext clientContext, IUserRepository userRepository, IClientRepository clientRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _clientContext = clientContext ?? throw new ArgumentNullException(nameof(clientContext));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
        }

        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;

            try
            {
                var client = GenerateClientForConnection(connectionId);

                await _clientRepository.CreateAsync(client);
                await _mediator.Publish(new ClientConnected(client));

                var connections = await _clientRepository.CountAsync(_ => _.Active && _.UserId == client.UserId);

                if (client.UserId.HasValue)
                {
                    var user = await _userRepository.GetByIdAsync(client.UserId.Value);

                    if (user != null)
                    {
                        await _userRepository.UpdateAsync(user, lastActiveAt: DateTimeOffset.UtcNow);
                        await _mediator.Publish(new UserConnected(user, connections, client));
                    }
                    else
                    {
                        _logger.LogWarning($"Unable to find the user with ID '{client.UserId}' while handling the connection for client '{client.Id}'.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"An error occurred while processing the connection: {ex}");
            }
            finally
            {
                _logger.LogDebug($"A client is connected: {connectionId}");
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;

            try
            {
                var client = await _clientRepository.GetAsync(predicate: _ => _.Active && _.ConnectionId == connectionId);
                if (client == null) return;

                await _clientRepository.DeactivateAsync(client);

                var connections = await _clientRepository.CountAsync(_ => _.Active && _.UserId == client.UserId);

                if (client.UserId.HasValue)
                {
                    var user = await _userRepository.GetByIdAsync(client.UserId.Value);

                    if (user != null)
                    {
                        await _userRepository.UpdateAsync(user, lastActiveAt: DateTimeOffset.UtcNow);
                        await _mediator.Publish(new UserDisconnected(user, connections, client));
                    }
                    else
                    {
                        _logger.LogWarning($"Unable to find the user with ID '{client.UserId}' while handling the disconnection for client '{client.Id}'.");
                    }
                }

                await _mediator.Publish(new ClientDisconnected(client));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"An error occurred while processing the disconnection: {ex}");
            }
            finally
            {
                _logger.LogDebug($"A client is disconnected: {connectionId}");
            }
        }

        protected Client GenerateClientForConnection(string connectionId)
        {
            return new Client
            {
                ConnectionId = connectionId,
                ConnectionTime = DateTimeOffset.UtcNow,
                Active = true,
                IpAddress = _clientContext.IpAddress,
                DeviceId = _clientContext.DeviceId,
                UserId = _clientContext.UserId,
                UserAgent = _clientContext.UserAgent
            };
        }
    }
}