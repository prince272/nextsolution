using MediatR;
using Microsoft.AspNetCore.SignalR;
using NextSolution.Core.Events.Users;

namespace NextSolution.Infrastructure.RealTime.Notifications
{
    public class UserSignedInNotification : INotificationHandler<UserSignedIn>
    {
        private readonly IHubContext<SignalRHub> _hubContext;

        public UserSignedInNotification(IHubContext<SignalRHub> hubContext)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        public async Task Handle(UserSignedIn notification, CancellationToken cancellationToken)
        {
            var message = new
            {
                UserId = notification.User.Id,
            };

            await _hubContext.Clients.All.SendAsync(notification.GetType().Name, message, cancellationToken);
        }
    }
}