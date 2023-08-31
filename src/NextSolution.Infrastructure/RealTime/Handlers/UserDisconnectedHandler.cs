using MediatR;
using Microsoft.AspNetCore.SignalR;
using NextSolution.Core.Events.Clients;
using NextSolution.Core.Models;
using NextSolution.Core.Repositories;
using NextSolution.Infrastructure.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Infrastructure.RealTime.Handlers
{
    public class UserDisconnectedHandler : INotificationHandler<UserDisconnected>
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IClientRepository _clientRepository;
        private readonly IModelMapper _modelMapper;

        public UserDisconnectedHandler(IHubContext<ChatHub> hubContext, IClientRepository clientRepository, IModelMapper modelMapper)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
            _modelMapper = modelMapper ?? throw new ArgumentNullException(nameof(modelMapper));
        }

        public async Task Handle(UserDisconnected notification, CancellationToken cancellationToken)
        {
            var otherUserIds = (await _clientRepository.GetManyAsync(predicate: _ => _.UserId != null, selector: _ => _.UserId, cancellationToken: cancellationToken)).Select(_ => _!.Value).ToArray();

            var message = new
            {
                UserId = notification.User.Id,
                Connections = notification.Connections,
                ClientId = notification.Client.Id,
            };

            await _hubContext.Clients.Users(otherUserIds.Select(_ => _.ToString())).SendAsync(nameof(UserDisconnected), message, cancellationToken);
        }
    }
}