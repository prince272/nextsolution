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
        public static IServiceCollection AddUserSession(this IServiceCollection services, Action<UserSessionOptions> options)
        {
            services.AddOptions<UserSessionOptions>().Configure<IHttpContextAccessor>((optionsInstance, httpContextAccessor) => {
                ConfigureUserSession(() => options(optionsInstance), optionsInstance, httpContextAccessor);
            });
            services.AddUserSession();
            return services;
        }

        public static IServiceCollection AddUserSession(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<UserSessionOptions>().Configure<IHttpContextAccessor>((options, httpContextAccessor) => {
                ConfigureUserSession(() => configuration.Bind(options), options, httpContextAccessor);
            });
            services.AddUserSession();
            return services;
        }

        private static void ConfigureUserSession(Action configure, UserSessionOptions options, IHttpContextAccessor httpContextAccessor)
        {
            configure();

            var context = httpContextAccessor?.HttpContext;
            var serverUrl = context != null ? string.Concat(context.Request.Scheme, "://", context.Request.Host.ToUriComponent()) : string.Empty;

            var separator = UserSessionOptions.ValueSeparator;

            options.Secret = !string.IsNullOrEmpty(options.Secret) ? options.Secret : Secrets.Key;
            options.Issuer = string.Join(separator, options.Issuer?.Split(separator).Append(serverUrl).Distinct().SkipWhile(string.IsNullOrEmpty).ToArray() ?? Array.Empty<string>());
            options.Audience = string.Join(separator, options.Audience?.Split(separator).Append(serverUrl).Distinct().SkipWhile(string.IsNullOrEmpty).ToArray() ?? Array.Empty<string>());
        }

        public static IServiceCollection AddUserSession(this IServiceCollection services)
        {
            services.AddScoped<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory>();
            services.AddScoped<IUserSessionFactory, UserSessionFactory>();
            services.AddScoped<IUserSessionStore, UserSessionStore>();
            services.AddScoped<IUserSessionContext, UserSessionContext>();
            return services;
        }
    }
}