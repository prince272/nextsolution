using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NextSolution.Core.Entities;
using System.Security.Claims;

namespace NextSolution.Infrastructure.Identity
{
    public class UserClaimsPrincipalFactory : UserClaimsPrincipalFactory<User, Role>
    {
        public UserClaimsPrincipalFactory(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IOptions<IdentityOptions> optionsAccessor) : base(userManager, roleManager, optionsAccessor)
        {
        }

        public async override Task<ClaimsPrincipal> CreateAsync(User user)
        {
            var principal = await base.CreateAsync(user);

            ((ClaimsIdentity)principal.Identity!).AddClaims(new Claim[] {
                new (ClaimTypes.GivenName, user.FirstName),
                new (ClaimTypes.Surname, user.LastName),
            });

            return principal;
        }
    }
}