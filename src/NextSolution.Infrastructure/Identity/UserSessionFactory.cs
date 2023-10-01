using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using NextSolution.Core.Entities;
using NextSolution.Core.Extensions.Identity;
using NextSolution.Core.Utilities;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NextSolution.Infrastructure.Identity
{
    public class UserSessionFactory : IUserSessionFactory
    {
        private readonly JwtBearerOptions _jwtBearerOptions;
        private readonly IOptions<UserSessionOptions> _userSessionOptions;
        private readonly IClientContext _clientContext;
        private readonly IUserClaimsPrincipalFactory<User> _userClaimsPrincipalFactory;
        private readonly ILogger<UserSessionFactory> _logger;

        public UserSessionFactory(
            IOptionsSnapshot<JwtBearerOptions> jwtBearerOptions,
            IOptions<UserSessionOptions> userSessionOptions,
            IClientContext clientContext,
            IUserClaimsPrincipalFactory<User> userClaimsPrincipalFactory, ILogger<UserSessionFactory> logger)
        {
            _jwtBearerOptions = jwtBearerOptions.Get(JwtBearerDefaults.AuthenticationScheme) ?? throw new ArgumentNullException(nameof(jwtBearerOptions));
            _userSessionOptions = userSessionOptions ?? throw new ArgumentNullException(nameof(userSessionOptions));
            _clientContext = clientContext ?? throw new ArgumentNullException(nameof(clientContext));
            _userClaimsPrincipalFactory = userClaimsPrincipalFactory ?? throw new ArgumentNullException(nameof(userClaimsPrincipalFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task<UserSessionInfo> GenerateAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var currentTime = DateTimeOffset.UtcNow;
            var claims = ((ClaimsIdentity)(await _userClaimsPrincipalFactory.CreateAsync(user)).Identity!).Claims;
            var (accessToken, accessTokenExpiresAt) = GenerateAccessToken(currentTime, _userSessionOptions.Value.AccessTokenExpiresIn, ref claims);
            var (refreshToken, refreshTokenExpiresAt) = GenerateRefreshToken(currentTime, _userSessionOptions.Value.RefreshTokenExpiresIn);

            return new UserSessionInfo
            {
                TokenType = JwtBearerDefaults.AuthenticationScheme,

                AccessToken = accessToken,
                AccessTokenExpiresAt = accessTokenExpiresAt,

                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = refreshTokenExpiresAt,

                User = new ClaimsPrincipal(new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme))
            };
        }

        private (string AccessToken, DateTimeOffset AccessTokenExpiresAt) GenerateAccessToken(DateTimeOffset currentTime, TimeSpan expiresIn, ref IEnumerable<Claim> claims)
        {
            if (claims == null) throw new ArgumentNullException(nameof(claims));

            var issuer = _clientContext.Issuer ?? throw new InvalidOperationException("Unable to determine the issuer.");
            var audience = _clientContext.Audience ?? _userSessionOptions.Value.GetAudiences().First();

            claims = claims.Concat(new Claim[] {
                new(JwtRegisteredClaimNames.Jti, AlgorithmHelper.GenerateStamp(), ClaimValueTypes.String, issuer),
                new(JwtRegisteredClaimNames.Iss, issuer, ClaimValueTypes.String, issuer),
                new(JwtRegisteredClaimNames.Iat, currentTime.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64, issuer),
            });

            if (_clientContext.DeviceId != null)
                claims = claims.Append(new(ClaimTypes.System, _clientContext.DeviceId, ClaimValueTypes.String, issuer));

            var expiresAt = currentTime.Add(expiresIn);

            var key = _jwtBearerOptions.TokenValidationParameters.IssuerSigningKey;
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(issuer, audience, claims, currentTime.DateTime, expiresAt.DateTime, creds);
            var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
            return (tokenValue, expiresAt);
        }

        private (string RefreshToken, DateTimeOffset RefreshTokenExpiresAt) GenerateRefreshToken(DateTimeOffset currentTime, TimeSpan expiresIn)
        {
            var issuer = _clientContext.Issuer ?? throw new InvalidOperationException("Unable to determine the issuer.");
            var audience = _clientContext.Audience ?? _userSessionOptions.Value.GetAudiences().First();

            var claims = new Claim[]
            {
                new(JwtRegisteredClaimNames.Jti, AlgorithmHelper.GenerateStamp(), ClaimValueTypes.String, issuer),
                new(JwtRegisteredClaimNames.Iss, issuer, ClaimValueTypes.String, issuer),
                new(JwtRegisteredClaimNames.Iat, currentTime.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64, issuer),
            };


            if (_clientContext.DeviceId != null)
                claims = claims.Append(new(ClaimTypes.System, _clientContext.DeviceId, ClaimValueTypes.String, issuer)).ToArray();

            var expiresAt = currentTime.Add(expiresIn);

            var key = _jwtBearerOptions.TokenValidationParameters.IssuerSigningKey;
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(issuer, audience, claims, currentTime.DateTime, expiresAt.DateTime, creds);
            var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
            return (tokenValue, expiresAt);
        }

        private string? GetDeviceId(ClaimsPrincipal principal)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            return principal.FindFirst(ClaimTypes.System) is Claim claim ? claim.Value : null;
        }

        private bool VerifyDevice(string deviceId)
        {
            if (deviceId == null) throw new ArgumentNullException(nameof(deviceId));

            return string.Equals(deviceId, _clientContext.DeviceId ?? "Unknown", StringComparison.Ordinal);
        }

        public Task<bool> ValidateAccessTokenAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(accessToken)) return Task.FromResult(false);

            try
            {
                var parameters = _jwtBearerOptions.TokenValidationParameters.Clone();
                var claimsPrincipal = new JwtSecurityTokenHandler().ValidateToken(accessToken, parameters, out var validatedToken);

                if (claimsPrincipal?.Claims == null || !claimsPrincipal.Claims.Any())
                    return Task.FromResult(false);

                var deviceId = GetDeviceId(claimsPrincipal);
                if (deviceId == null || !VerifyDevice(deviceId)) return Task.FromResult(false);

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to validate {nameof(accessToken)}: `{accessToken}`.");

                return Task.FromResult(false);
            }
        }

        public Task<bool> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) return Task.FromResult(false);

            try
            {
                var parameters = _jwtBearerOptions.TokenValidationParameters.Clone();
                var claimsPrincipal = new JwtSecurityTokenHandler().ValidateToken(refreshToken, parameters, out var validatedToken);

                if (claimsPrincipal?.Claims == null || !claimsPrincipal.Claims.Any())
                    return Task.FromResult(false);

                var deviceId = GetDeviceId(claimsPrincipal);
                if (deviceId == null || !VerifyDevice(deviceId)) return Task.FromResult(false);

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to validate {nameof(refreshToken)}: `{refreshToken}`.");

                return Task.FromResult(false);
            }
        }
    }
}