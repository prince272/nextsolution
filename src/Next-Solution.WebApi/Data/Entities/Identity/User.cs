using Microsoft.AspNetCore.Identity;

namespace Next_Solution.WebApi.Data.Entities.Identity
{
    public class User : IdentityUser<string>
    {
        public User()
        {
            Id = Guid.NewGuid().ToString("N");
        }

        public User(string userName) : this()
        {
            UserName = userName;
        }

        public string FullName => $"{FirstName} {LastName}".Trim();

        public string FirstName { get; set; } = null!;

        public string? LastName { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset LastActiveAt { get; set; }

        public bool PasswordConfigured { get; set; }
    }

    public class UserRole : IdentityUserRole<string>
    {
        public virtual User User { get; set; } = null!;

        public virtual Role Role { get; set; } = null!;
    }
}
