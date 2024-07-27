namespace NextSolution.WebApi.Providers.JwtBearer
{
    public class JwtProviderOptions
    {
        public string Secret { set; get; } = null!;

        public string Issuer { set; get; } = null!;

        public string Audience { set; get; } = null!;

        public TimeSpan AccessTokenExpiresIn { set; get; } = TimeSpan.FromMinutes(15);

        public TimeSpan RefreshTokenExpiresIn { set; get; } = TimeSpan.FromDays(30);
    }
}
