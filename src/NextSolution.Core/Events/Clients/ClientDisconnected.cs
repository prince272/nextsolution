using MediatR;
using NextSolution.Core.Entities;

namespace NextSolution.Core.Events.Clients
{
    public class ClientDisconnected : INotification
    {
        public ClientDisconnected(Client client)
        {
            Client = client;
        }

        public Client Client { get; }
    }
}
