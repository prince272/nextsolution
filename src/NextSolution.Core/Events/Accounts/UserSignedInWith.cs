using MediatR;
using NextSolution.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Events.Accounts
{
    public class UserSignedInWith : INotification
    {
        public UserSignedInWith(User user, string provider)
        {
            User = user;
            Provider = provider;
        }

        public User User { get; set; }

        public string Provider { get; }
    }
}
