using MediatR;
using Microsoft.AspNetCore.SignalR;
using NextSolution.Core.Events.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Infrastructure.RealTime.Handlers
{
    public class UserSignedOutHandler : INotificationHandler<UserSignedOut>
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public UserSignedOutHandler(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        public async Task Handle(UserSignedOut notification, CancellationToken cancellationToken)
        {
            var message = new
            {
                UserId = notification.User.Id,
            };

            await _hubContext.Clients.All.SendAsync(notification.GetType().Name, message, cancellationToken);
        }
    }
}