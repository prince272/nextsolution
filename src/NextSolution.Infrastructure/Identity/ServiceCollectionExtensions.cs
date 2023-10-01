using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NextSolution.Core.Entities;
using NextSolution.Core.Extensions.Identity;
using NextSolution.Core.Utilities;

namespace NextSolution.Infrastructure.Identity
{
    public static class ServiceCollectionExtensions
    {
        public static AuthenticationBuilder AddBearer(this AuthenticationBuilder builder, Action<UserSessionOptions> options)
        {
            builder.Services.AddOptions<UserSessionOptions>().Configure<IHttpContextAccessor>((optionsInstance, httpContextAccessor) =>
            {
                ConfigureBearer(() => options(optionsInstance), optionsInstance, httpContextAccessor);
            });
            builder.AddBearer();
            return builder;
        }

        private static void ConfigureBearer(Action configure, UserSessionOptions options, IHttpContextAccessor httpContextAccessor)
        {
            configure();

            var context = httpContextAccessor?.HttpContext;
            var serverUrl = context != null ? string.Concat(context.Request.Scheme, "://", context.Request.Host.ToUriComponent()) : string.Empty;

            var separator = UserSessionOptions.ValueSeparator;

            options.Secret = !string.IsNullOrEmpty(options.Secret) ? options.Secret : AlgorithmHelper.Secret;
            options.Issuer = string.Join(separator, (options.Issuer ?? string.Empty).Split(separator).Append(serverUrl).Distinct().SkipWhile(string.IsNullOrEmpty).ToArray());
            options.Audience = string.Join(separator, (options.Audience ?? string.Empty).Split(separator).Append(serverUrl).Distinct().SkipWhile(string.IsNullOrEmpty).ToArray());

            options.AccessTokenExpiresIn = options.AccessTokenExpiresIn != TimeSpan.Zero ? options.AccessTokenExpiresIn : TimeSpan.FromDays(1);
            options.RefreshTokenExpiresIn = options.RefreshTokenExpiresIn != TimeSpan.Zero ? options.RefreshTokenExpiresIn : TimeSpan.FromDays(90);
        }

        public static AuthenticationBuilder AddBearer(this AuthenticationBuilder builder)
        {
            builder.Services.AddScoped<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory>();
            builder.Services.AddScoped<IUserSessionFactory, UserSessionFactory>();

            builder.Services.ConfigureOptions<ConfigureJwtBearerOptions>();
            return builder.AddJwtBearer();
        }

        public static IServiceCollection AddClientContext(this IServiceCollection services)
        {
            return services.AddScoped<IClientContext, ClientContext>();
        }
    }
}