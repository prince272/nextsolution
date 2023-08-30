using MediatR;
using NextSolution.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Events.Clients
{
    public class UserDisconnected : INotification
    {
        public UserDisconnected(User user, Client client)
        {
            User = user;
            Client = client;
        }

        public User User { get; set; }

        public Client Client { get; set; }
    }
}
