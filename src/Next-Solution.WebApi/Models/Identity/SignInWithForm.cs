using System.Security.Claims;

namespace Next_Solution.WebApi.Models.Identity
{
    public class SignInWithForm
    {
        public SignInProvider Provider { get; set; }

        public string ProviderKey { get; set; } = null!;

        public string? ProviderDisplayName { get; set; }

        public ClaimsPrincipal Principal { get; set; } = null!;
    }
}
