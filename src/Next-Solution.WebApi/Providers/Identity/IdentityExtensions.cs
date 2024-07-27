using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Next_Solution.WebApi.Data.Entities.Identity;

namespace Next_Solution.WebApi.Providers.Identity
{
    public static class IdentityExtensions
    {
        public static Task<TUser?> FindByPhoneNumberAsync<TUser>(this UserManager<TUser> userManager, string phoneNumber)
            where TUser : IdentityUser<string>
        {
            if (userManager is null) throw new ArgumentNullException(nameof(userManager));
            if (phoneNumber is null) throw new ArgumentNullException(nameof(phoneNumber));

            return userManager.Users.FirstOrDefaultAsync(_ => _.PhoneNumber == phoneNumber);
        }

        public static string? GetSecurityStamp(this UserManager<User> userManager, ClaimsPrincipal principal)
        {
            if (userManager is null) throw new ArgumentNullException(nameof(userManager));
            if (principal is null) throw new ArgumentNullException(nameof(principal));
            return principal.FindFirst(userManager.Options.ClaimsIdentity.SecurityStampClaimType) is Claim claim ? claim.Value : null;
        }

        public static string GetMessage(this IEnumerable<IdentityError> errors)
        {
            if (errors == null) throw new ArgumentNullException(nameof(errors));
            return $"Identity operation failed:{string.Concat(errors.Select(x => $"{Environment.NewLine} -- {x.Code}: {x.Description}"))}";
        }
    }
}