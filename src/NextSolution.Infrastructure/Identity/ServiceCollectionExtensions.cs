using DeviceId;
using Humanizer.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NextSolution.Core.Entities;
using NextSolution.Core.Extensions.Identity;
using NextSolution.Core.Repositories;
using NextSolution.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Infrastructure.Identity
{
    public static class ServiceCollectionExtensions
    {
        public static AuthenticationBuilder AddBearer(this AuthenticationBuilder builder, Action<UserSessionOptions> options)
        {
            builder.Services.AddOptions<UserSessionOptions>().Configure<IHttpContextAccessor>((optionsInstance, httpContextAccessor) => {
                ConfigureBearer(() => options(optionsInstance), optionsInstance, httpContextAccessor);
            });
            builder.AddBearer();
            return builder;
        }

        public static AuthenticationBuilder AddBearer(this AuthenticationBuilder builder, IConfiguration configuration)
        {
            builder.Services.AddOptions<UserSessionOptions>().Configure<IHttpContextAccessor>((options, httpContextAccessor) => {
                ConfigureBearer(() => configuration.Bind(options), options, httpContextAccessor);
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
            options.RefreshTokenExpiresIn = options.AccessTokenExpiresIn != TimeSpan.Zero ? options.AccessTokenExpiresIn : TimeSpan.FromDays(90);
        }

        public static AuthenticationBuilder AddBearer(this AuthenticationBuilder builder)
        {
            builder.Services.AddScoped<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory>();
            builder.Services.AddScoped<IUserSessionFactory, UserSessionFactory>();
            builder.Services.AddScoped<IUserSessionStore, UserSessionStore>();
            builder.Services.AddScoped<IUserContext, UserContext>();

            builder.Services.ConfigureOptions<ConfigureJwtBearerOptions>();
            return builder.AddJwtBearer();
        }
    }
}