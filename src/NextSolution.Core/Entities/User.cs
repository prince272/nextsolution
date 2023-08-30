using Microsoft.AspNetCore.Identity;
using NextSolution.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public string FirstName { get; set; } = default!;

        public string LastName { get; set; } = default!;

        public bool Active { get; set; }

        public DateTimeOffset LastActiveAt { get; set; }

        public virtual ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
    }

    public class  UserRole : IdentityUserRole<long>, IEntity
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
