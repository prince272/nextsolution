namespace NextSolution.Server.Providers.JwtBearer
{
    public class JwtTokenInfo
    {
        public string TokenType { get; set; } = null!;
        public string AccessToken { get; set; } = null!;
        public DateTimeOffset AccessTokenExpiresAt { get; set; }
        public string RefreshToken { get; set; } = null!;
        public DateTimeOffset RefreshTokenExpiresAt { get; set; }
    }
}
