using MediatR;
using NextSolution.Core.Entities;

namespace NextSolution.Core.Events.Users
{
    public class UserSignedIn : INotification
    {
        public UserSignedIn(User user)
        {
            User = user;
        }

        public User User { get; set; }
    }
}