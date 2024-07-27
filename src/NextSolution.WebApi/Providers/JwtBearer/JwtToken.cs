using NextSolution.WebApi.Data.Entities.Identity;

namespace NextSolution.WebApi.Providers.JwtBearer
{
    public class JwtToken
    {
        public JwtToken()
        {
            Id = Guid.NewGuid().ToString("N");
        }

        public virtual User User { get; set; } = null!;

        public string UserId { get; set; } = null!;

        public string Id { get; set; }

        public string AccessTokenHash { get; set; } = null!;

        public DateTimeOffset AccessTokenExpiresAt { get; set; }

        public string RefreshTokenHash { get; set; } = null!;

        public DateTimeOffset RefreshTokenExpiresAt { get; set; }
    }
}
