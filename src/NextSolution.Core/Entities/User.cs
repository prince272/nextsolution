using Microsoft.AspNetCore.Identity;
using NextSolution.Core.Shared;

namespace NextSolution.Core.Entities
{
    public class User : IdentityUser<long>, IEntity
    {
        public User()
        {
        }

        public User(string userName) : base(userName)
        {
        }

        public bool EmailRequired { get; set; }

        public bool PhoneNumberRequired { get; set; }

        public string FirstName { get; set; } = default!;

        public string LastName { get; set; } = default!;

        public string? Bio { get; set; }

        public Media? Avatar { get; set; }

        public long? AvatarId { get; set; }

        public bool Active { get; set; }

        public DateTimeOffset LastActiveAt { get; set; }

        public virtual ICollection<UserRole> Roles { get; set; } = new List<UserRole>();

        public virtual ICollection<Client> Clients { get; set; } = new List<Client>();
    }

    public class UserRole : IdentityUserRole<long>, IEntity
    {
        public virtual User User { get; set; } = default!;

        public virtual Role Role { get; set; } = default!;

        long IEntity.Id { get; }
    }

    public class UserSession : IEntity
    {
        public virtual User User { get; set; } = default!;

        public long UserId { get; set; }

        public long Id { get; set; }

        public string AccessTokenHash { get; set; } = default!;

        public DateTimeOffset AccessTokenExpiresAt { get; set; }

        public string RefreshTokenHash { get; set; } = default!;

        public DateTimeOffset RefreshTokenExpiresAt { get; set; }
    }
}
