using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Events.Accounts
{
    public class UserSignedUp : INotification
    {
        public UserSignedUp(long userId)
        {
            UserId = userId;
        }

        public long UserId { get; set; }    
    }
}
