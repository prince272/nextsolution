using MediatR;
using NextSolution.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Events.Clients
{
    public class ClientConnected : INotification
    {
        public ClientConnected(Client client)
        {
            Client = client;
        }

        public Client Client { get; }
    }
}