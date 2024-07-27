using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Next_Solution.WebApi.Data.Entities.Identity;
using Next_Solution.WebApi.Data;
using Next_Solution.WebApi.Helpers;

namespace Next_Solution.WebApi.Providers.JwtBearer
{
    public class JwtProvider : IJwtProvider
    {
        private readonly IOptions<JwtProviderOptions> _jwtProviderOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserClaimsPrincipalFactory<User> _userClaimsPrincipalFactory;
        private readonly ApplicationDbContext _applicationDbContext;

        public JwtProvider(
            IOptions<JwtProviderOptions> jwtBearerProviderOptions,
            IHttpContextAccessor httpContextAccessor,
            IUserClaimsPrincipalFactory<User> userClaimsPrincipalFactory,
            ApplicationDbContext applicationDbContext)
        {
            _jwtProviderOptions = jwtBearerProviderOptions ?? throw new ArgumentNullException(nameof(jwtBearerProviderOptions));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _userClaimsPrincipalFactory = userClaimsPrincipalFactory ?? throw new ArgumentNullException(nameof(userClaimsPrincipalFactory));
            _applicationDbContext = applicationDbContext ?? throw new ArgumentNullException(nameof(applicationDbContext));
        }

        public async Task<JwtTokenInfo> GenerateTokenAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));

            var currentTime = DateTimeOffset.UtcNow;
            var httpContext = (_httpContextAccessor?.HttpContext) ?? throw new InvalidOperationException("Unable to determine the current HttpContext.");
            string issuer = string.Concat(httpContext.Request.Scheme, "://", httpContext.Request.Host.ToUriComponent()).ToLower();
            string audience = httpContext.Request.Headers.Referer.ToString();
            audience = !string.IsNullOrEmpty(audience) ? new Uri(audience).GetLeftPart(UriPartial.Authority) : issuer;

            var claims = (await _userClaimsPrincipalFactory.CreateAsync(user)).Claims.ToList();
            var (accessToken, accessTokenExpiresAt, _) = GenerateToken(currentTime, _jwtProviderOptions.Value.AccessTokenExpiresIn, issuer, audience, claims);
            var (refreshToken, refreshTokenExpiresAt, _) = GenerateToken(currentTime, _jwtProviderOptions.Value.RefreshTokenExpiresIn, issuer, audience);

            await _applicationDbContext.AddAsync(new JwtToken
            {
                UserId = user.Id,

                Id = Guid.NewGuid().ToString(),

                AccessTokenHash = HashHelper.GenerateSHA256Hash(accessToken),
                RefreshTokenHash = HashHelper.GenerateSHA256Hash(refreshToken),

                AccessTokenExpiresAt = accessTokenExpiresAt,
                RefreshTokenExpiresAt = refreshTokenExpiresAt
            }, cancellationToken);
            await _applicationDbContext.SaveChangesAsync(cancellationToken);

            return new JwtTokenInfo
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessTokenExpiresAt,
                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = refreshTokenExpiresAt,
            };
        }

        private (string Token, DateTimeOffset TokenExpiresAt, IEnumerable<Claim> TokenClaims) GenerateToken(DateTimeOffset time, TimeSpan expiresIn, string issuer, string audience, IEnumerable<Claim>? claims = null)
        {
            if (issuer is null) throw new ArgumentNullException(nameof(issuer));
            if (audience is null) throw new ArgumentNullException(nameof(audience));

            var tokenClaims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString(), ClaimValueTypes.String, issuer),
                new(JwtRegisteredClaimNames.Iss, issuer, ClaimValueTypes.String, issuer),
                new(JwtRegisteredClaimNames.Iat, time.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64, issuer)
            };
            if (claims != null) tokenClaims.AddRange(claims);

            var expiresAt = time.Add(expiresIn);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtProviderOptions.Value.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(issuer, audience, claims, time.UtcDateTime, expiresAt.UtcDateTime, creds);
            var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
            return (tokenValue, expiresAt, tokenClaims);
        }

        public async Task InvalidateTokensAsync(User user, string? refreshToken = null, bool allowMultipleTokens = true, CancellationToken cancellationToken = default)
        {
            var currentTime = DateTimeOffset.UtcNow;
            var refreshTokenHash = refreshToken != null ? HashHelper.GenerateSHA256Hash(refreshToken) : null;

            // Get all invalid tokens in one query
            var invalidTokensQuery = _applicationDbContext.Set<JwtToken>()
                .Where(token => token.UserId == user.Id &&
                    (token.AccessTokenExpiresAt < currentTime || token.RefreshTokenExpiresAt < currentTime ||
                     refreshTokenHash != null && token.RefreshTokenHash == refreshTokenHash));

            await invalidTokensQuery.ForEachAsync(token => _applicationDbContext.Remove(token), cancellationToken);

            // Optionally, remove all tokens if multiple tokens are not allowed
            if (!allowMultipleTokens)
            {
                var multipleTokensQuery = _applicationDbContext.Set<JwtToken>()
                    .Where(token => token.UserId == user.Id);

                await multipleTokensQuery.ForEachAsync(token => _applicationDbContext.Remove(token), cancellationToken);
            }

            // Save changes asynchronously
            await _applicationDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<User?> FindUserByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            if (refreshToken == null) throw new ArgumentNullException(nameof(refreshToken));

            var refreshTokenHash = HashHelper.GenerateSHA256Hash(refreshToken);
            var currentTime = DateTimeOffset.UtcNow;

            var token = await _applicationDbContext.Set<JwtToken>()
                .FirstOrDefaultAsync(token => token.RefreshTokenHash == refreshTokenHash &&
                                   token.RefreshTokenExpiresAt > currentTime, cancellationToken);

            if (token == null) return null;

            return await _applicationDbContext.FindAsync<User>(keyValues: new object[] { token.UserId }, cancellationToken);
        }

        public async Task<bool> ValidateAccessTokenAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            if (accessToken == null) throw new ArgumentNullException(nameof(accessToken));

            var accessTokenHash = HashHelper.GenerateSHA256Hash(accessToken);
            var currentTime = DateTimeOffset.UtcNow;

            var result = await _applicationDbContext.Set<JwtToken>()
                .AnyAsync(token => token.AccessTokenHash == accessTokenHash &&
                                   token.AccessTokenExpiresAt > currentTime &&
                                   token.RefreshTokenExpiresAt > currentTime, cancellationToken);
            return result;
        }
    }

    public interface IJwtProvider
    {
        Task<JwtTokenInfo> GenerateTokenAsync(User user, CancellationToken cancellationToken = default);

        Task InvalidateTokensAsync(User user, string? refreshToken = null, bool allowMultipleTokens = true, CancellationToken cancellationToken = default);

        Task<User?> FindUserByRefreshTokenAsync(string token, CancellationToken cancellationToken = default);

        Task<bool> ValidateAccessTokenAsync(string accessToken, CancellationToken cancellationToken = default);
    }
}