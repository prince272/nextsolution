using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Next_Solution.WebApi.Data.Entities.Identity;
using System.Security.Claims;
using System.Text;

namespace Next_Solution.WebApi.Providers.JwtBearer
{
    public class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
    {
        private readonly IOptions<JwtProviderOptions> _jwtBearerProviderOptions;

        public ConfigureJwtBearerOptions(IOptions<JwtProviderOptions> jwtBearerProviderOptions)
        {
            _jwtBearerProviderOptions = jwtBearerProviderOptions ?? throw new ArgumentNullException(nameof(jwtBearerProviderOptions));
        }

        public void Configure(string? name, JwtBearerOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (name == JwtBearerDefaults.AuthenticationScheme) Configure(options);
        }

        public void Configure(JwtBearerOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers = _jwtBearerProviderOptions.Value.Issuer.Split(";", StringSplitOptions.RemoveEmptyEntries).ToArray(),

                ValidateAudience = true,
                ValidAudiences = _jwtBearerProviderOptions.Value.Audience.Split(";", StringSplitOptions.RemoveEmptyEntries).ToArray(),

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtBearerProviderOptions.Value.Secret)),

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
            };
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();

                    // Log detailed information about the failure
                    logger.LogError(context.Exception,
                        "Authentication failed. Exception message: {Message}, Stack Trace: {StackTrace}, Source: {Source}",
                        context.Exception.Message,
                        context.Exception.StackTrace,
                        context.Exception.Source);

                    return Task.CompletedTask;
                },
                OnMessageReceived = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();

                    // Log detailed information about the received message
                    logger.LogInformation("Message received. Scheme: {Scheme}, Token: {Token}", context.Scheme.Name, context.Token ?? "No token provided");

                    return Task.CompletedTask;
                },
                OnTokenValidated = async context =>
                {
                    var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
                    var jwtProvider = context.HttpContext.RequestServices.GetRequiredService<IJwtProvider>();
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(JwtBearerEvents));

                    var claimsPrincipal = context.Principal;

                    if (claimsPrincipal?.Claims == null || !claimsPrincipal.Claims.Any())
                    {
                        logger.LogWarning("No claims found in the token.");
                        context.Fail("This is not our issued token. It has no claims.");
                        return;
                    }

                    var userId = userManager.GetUserId(claimsPrincipal);
                    var user = !string.IsNullOrEmpty(userId) ? await userManager.FindByIdAsync(userId) : null;

                    var securityStamp = claimsPrincipal.FindFirst(userManager.Options.ClaimsIdentity.SecurityStampClaimType) is Claim claim ? claim.Value : null;

                    if (user == null || !string.Equals(user.SecurityStamp, securityStamp, StringComparison.Ordinal))
                    {
                        // user has changed his/her password/roles/active
                        logger.LogWarning("User has changed his/her password/roles/active.");
                        context.Fail("This token is expired. Please login again.");
                        return;
                    }

                    // JwtSecurityToken in .NET 8
                    // source: https://stackoverflow.com/questions/77550892/jwtsecuritytoken-in-net-8
                    var accessToken = context.SecurityToken as JsonWebToken;

                    if (accessToken == null || string.IsNullOrWhiteSpace(accessToken.EncodedToken) || !await jwtProvider.ValidateAccessTokenAsync(accessToken.EncodedToken))
                    {
                        logger.LogWarning("Invalid or missing token in the request.");
                        context.Fail("Invalid or missing token in the request.");
                        return;
                    }

                    var activityThreshold = TimeSpan.FromMinutes(1);
                    var currentTime = DateTimeOffset.UtcNow;

                    if (currentTime - user.LastActiveAt >= activityThreshold)
                    {
                        user.LastActiveAt = currentTime;
                        await userManager.UpdateAsync(user);

                        logger.LogInformation("User activity updated. UserId: {UserId}, UserName: {UserName}", user.Id, user.UserName);
                    }

                    logger.LogInformation("Token validated. UserId: {UserId}, UserName: {UserName}", user.Id, user.UserName);
                },
                OnChallenge = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();

                    // Log detailed information about the challenge
                    logger.LogError("OnChallenge error. Error: {Error}, ErrorDescription: {ErrorDescription}, RequestPath: {RequestPath}, ResponseStatusCode: {StatusCode}",
                        context.Error,
                        context.ErrorDescription,
                        context.Request.Path,
                        context.Response.StatusCode);

                    return Task.CompletedTask;
                },
            };
        }
    }
}
