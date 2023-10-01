using MediatR;
using NextSolution.Core.Entities;

namespace NextSolution.Core.Events.Users
{
    public class UserSignedOut : INotification
    {
        public UserSignedOut(User user)
        {
            User = user;
        }

        public User User { get; set; }
    }
}