using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Entities
{
    public class User : IdentityUser<long>
    {
        public User()
        {
        }

        public User(string userName) : base(userName)
        {
        }

        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }

    public class  UserRole : IdentityUserRole<long>
    {
        public virtual User User { get; set; } = default!;

        public virtual Role Role { get; set; } = default!;
    }
}
