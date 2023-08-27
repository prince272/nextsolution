using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Events.Accounts
{
    public class UserSignedInWith : INotification
    {
        public UserSignedInWith(long userId, string provider)
        {
            UserId = userId;
            Provider = provider;
        }

        public long UserId { get; set; }

        public string Provider { get; }
    }
}
