using MediatR;
using Microsoft.AspNetCore.SignalR;
using NextSolution.Core.Events.Users;
using NextSolution.Core.Models;
using NextSolution.Core.Repositories;
using NextSolution.Infrastructure.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Infrastructure.RealTime.Notifications
{
    public class UserConnectedNotification : INotificationHandler<UserConnected>
    {
        private readonly IHubContext<SignalRHub> _hubContext;

        public UserConnectedNotification(IHubContext<SignalRHub> hubContext)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        public async Task Handle(UserConnected notification, CancellationToken cancellationToken)
        {
            var message = new
            {
                UserId = notification.User.Id,
                notification.Connections,
                ClientId = notification.Client.Id,
            };

            await _hubContext.Clients.All.SendAsync(notification.GetType().Name, message, cancellationToken);
        }
    }
}