using DeviceId;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
        public static IdentityBuilder AddUserSession(this IdentityBuilder builder, Action<UserSessionOptions> options)
        {
            builder.Services.Configure(options);
            builder.Services.AddScoped<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory>();
            builder.Services.AddScoped<IUserSessionFactory, UserSessionFactory>();
            builder.Services.AddScoped<IUserSessionStore, UserSessionStore>();
            builder.Services.AddScoped<IUserSessionContext, UserSessionContext>();
            return builder;
        }
    }
}