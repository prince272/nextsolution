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
    public class UserConnectedHandler : INotificationHandler<UserConnected>
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IClientRepository _clientRepository;
        private readonly IModelMapper _modelMapper;

        public UserConnectedHandler(IHubContext<ChatHub> hubContext, IClientRepository clientRepository, IModelMapper modelMapper)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
            _modelMapper = modelMapper ?? throw new ArgumentNullException(nameof(modelMapper));
        }

        public async Task Handle(UserConnected notification, CancellationToken cancellationToken)
        {
            var activeUserIds = (await _clientRepository.GetManyAsync(predicate: _ => _.UserId != null, selector: _ => _.UserId)).Select(_ => _.ToString()!).ToArray();

            var userModel = await _modelMapper.MapAsync(notification.User);
            await _hubContext.Clients.Users(activeUserIds).SendAsync(nameof(UserConnected), userModel, cancellationToken);
        }
    }
}