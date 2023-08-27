using MediatR;
using NextSolution.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Events.Accounts
{
    public class UserSignedIn : INotification
    {
        public UserSignedIn(long userId)
        {
            UserId = userId;
        }

        public long UserId { get; set; }
    }
}