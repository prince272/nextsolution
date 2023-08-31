using MediatR;
using NextSolution.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Events.Clients
{
    public class UserConnected : INotification
    {
        public UserConnected(User user, long connections, Client client)
        {
            User = user;
            Connections = connections;
            Client = client;
        }

        public User User { get; set; }

        public long Connections { get; set; }

        public Client Client { get; set; }
    }
}
