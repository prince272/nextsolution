using MediatR;
using NextSolution.Core.Events.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Infrastructure.RealTime.Handlers
{
    public class UserSignedUpHandler : INotificationHandler<UserSignedUp>
    {
        public Task Handle(UserSignedUp notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}