using System.Security.Claims;

namespace NextSolution._1.Server.Models.Identity
{
    public class SignInWithForm 
    {
        public SignInProvider Provider { get; set; }

        public string ProviderKey { get; set; } = null!;

        public string? ProviderDisplayName { get; set; }

        public ClaimsPrincipal Principal { get; set; } = null!;
    }
}
