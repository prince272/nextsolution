using MediatR;
using NextSolution.Core.Entities;

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