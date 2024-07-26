using NextSolution._1.Server.Helpers;
using DeviceId;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace NextSolution._1.Server.Providers.JwtBearer
{
    public class ConfigureJwtProviderOptions : IConfigureOptions<JwtProviderOptions>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ConfigureJwtProviderOptions(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public void Configure(JwtProviderOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            options.Secret ??= HashHelper.GenerateSHA256Hash(new DeviceIdBuilder()
                  .AddMachineName()
                  .AddOsVersion()
                  .AddUserName()
                  .AddFileToken(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, $"jwt-secret.txt")).ToString());

            var httpContext = (_httpContextAccessor?.HttpContext) ?? throw new InvalidOperationException("Unable to determine the current HttpContext.");
            string currentOrigin = string.Concat(httpContext.Request.Scheme, "://", httpContext.Request.Host.ToUriComponent()).ToLower();

            options.Issuer ??= currentOrigin;
            options.Audience ??= currentOrigin;
        }
    }
}
