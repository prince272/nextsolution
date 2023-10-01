using Microsoft.AspNetCore.Identity;
using NextSolution.Core.Shared;

namespace NextSolution.Core.Entities
{
    public class Role : IdentityRole<long>, IEntity
    {
        public Role()
        {
        }

        public Role(string roleName) : base(roleName)
        {
        }

        public virtual ICollection<UserRole> Users { get; set; } = new List<UserRole>();

        public const string Admin = nameof(Admin);

        public const string Member = nameof(Member);

        public static IEnumerable<string> All => new[] { Admin, Member };
    }
}
