using MediatR;
using NextSolution.Core.Entities;

namespace NextSolution.Core.Events.Users
{
    public class UserSignedUp : INotification
    {
        public UserSignedUp(User user)
        {
            User = user;
        }

        public User User { get; set; }
    }
}
