using System.Security.Claims;

namespace NextSolution.Core.Extensions.Identity
{
    public class UserSessionInfo
    {
        public string AccessToken { get; set; } = default!;

        public DateTimeOffset AccessTokenExpiresAt { get; set; }

        public string RefreshToken { get; set; } = default!;

        public DateTimeOffset RefreshTokenExpiresAt { get; set; }

        public string TokenType { get; set; } = default!;

        public ClaimsPrincipal User { get; set; } = default!;
    }
}
