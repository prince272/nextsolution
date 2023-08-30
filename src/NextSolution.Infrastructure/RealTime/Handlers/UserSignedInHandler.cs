using MediatR;
using NextSolution.Core.Events.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Infrastructure.RealTime.Handlers
{
    public class UserSignedInHandler : INotificationHandler<UserSignedIn>
    {
        public Task Handle(UserSignedIn notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}