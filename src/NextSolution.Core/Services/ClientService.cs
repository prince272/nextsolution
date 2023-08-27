using MediatR;
using NextSolution.Core.Entities;
using NextSolution.Core.Events.Clients;
using NextSolution.Core.Extensions.Identity;
using NextSolution.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Services
{
    public interface IClientService
    {
        Task ConnectAsync(string connectionId, CancellationToken cancellationToken = default);
        Task DisconnectAsync(CancellationToken cancellationToken = default);
        Task DisconnectAsync(string connectionId, CancellationToken cancellationToken = default);
        Task<bool> IsUserOnlineAsync(long userId, CancellationToken cancellationToken = default);
    }

    public class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMediator _mediator;
        private readonly IUserContext _userContext;

        public ClientService(IClientRepository clientRepository, IUserRepository userRepository, IMediator mediator, IUserContext userContext)
        {
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        public async Task<bool> IsUserOnlineAsync(long userId, CancellationToken cancellationToken = default)
        {
            return await _clientRepository.AnyAsync(_ => _.UserId == userId, cancellationToken);
        }

        public async Task ConnectAsync(string connectionId, CancellationToken cancellationToken = default)
        {
            if (connectionId == null) throw new ArgumentNullException(nameof(connectionId));

            var client = GenerateClient(connectionId);
            var userId = client.UserId;

            await _clientRepository.CreateAsync(client, cancellationToken);
            await _mediator.Publish(new ClientConnected(client), cancellationToken);

            if (userId.HasValue && await IsUserOnlineAsync(userId.Value, cancellationToken))
            {
                await _mediator.Publish(new UserConnected((await _userRepository.FindByIdAsync(userId.Value, cancellationToken))!, client), cancellationToken);
            }
        }

        public async Task DisconnectAsync(string connectionId, CancellationToken cancellationToken = default)
        {
            if (connectionId == null) throw new ArgumentNullException(nameof(connectionId));

            var client = await _clientRepository.FindAsync(predicate: _ => _.ConnectionId == connectionId, cancellationToken: cancellationToken);
            if (client == null) return;

            await _clientRepository.DeleteAsync(client, cancellationToken);

            var userId = client.UserId;

            if (userId.HasValue && !(await IsUserOnlineAsync(userId.Value, cancellationToken)))
            {
                await _mediator.Publish(new UserDisconnected((await _userRepository.FindByIdAsync(userId.Value, cancellationToken))!, client), cancellationToken);
            }

            await _mediator.Publish(new ClientDisconnected(client), cancellationToken);
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            var connectionIds = await _clientRepository.FindAllAsync(selector: _ => _.ConnectionId, cancellationToken: cancellationToken);
            foreach (var connectionId in connectionIds)
                await DisconnectAsync(connectionId, cancellationToken);
        }

        protected Client GenerateClient(string connectionId)
        {
            return new Client
            {
                ConnectionId = connectionId,
                IpAddress = _userContext.IpAddress,
                DeviceId = _userContext.DeviceId,
                UserId = _userContext.UserId,
                UserAgent = _userContext.UserAgent
            };
        }
    }
}