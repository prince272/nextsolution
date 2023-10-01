using MediatR;
using NextSolution.Core.Entities;

namespace NextSolution.Core.Events.Users
{
    public class UserDisconnected : INotification
    {
        public UserDisconnected(User user, long connections, Client client)
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
