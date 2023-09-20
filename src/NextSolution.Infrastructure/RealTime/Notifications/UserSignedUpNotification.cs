using MediatR;
using Microsoft.AspNetCore.SignalR;
using NextSolution.Core.Events.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Infrastructure.RealTime.Notifications
{
    public class UserSignedUpNotification : INotificationHandler<UserSignedUp>
    {
        private readonly IHubContext<SignalRHub> _hubContext;

        public UserSignedUpNotification(IHubContext<SignalRHub> hubContext)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        public async Task Handle(UserSignedUp notification, CancellationToken cancellationToken)
        {
            var message = new
            {
                UserId = notification.User.Id,
            };

            await _hubContext.Clients.All.SendAsync(notification.GetType().Name, message, cancellationToken);
        }
    }
}