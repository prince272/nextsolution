using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Next_Solution.WebApi.Providers.JwtBearer
{
    public static class JwtProviderExtensions
    {
        public static AuthenticationBuilder AddJwtProvider(this AuthenticationBuilder authenticationBuilder, Action<JwtProviderOptions> options)
        {
            if (authenticationBuilder == null) throw new ArgumentNullException(nameof(authenticationBuilder));
            if (options == null) throw new ArgumentNullException(nameof(options));

            var services = authenticationBuilder.Services;
            services.Configure(options);
            return authenticationBuilder.AddJwtProvider();
        }

        public static AuthenticationBuilder AddJwtProvider(this AuthenticationBuilder authenticationBuilder)
        {
            if (authenticationBuilder == null) throw new ArgumentNullException(nameof(authenticationBuilder));

            var services = authenticationBuilder.Services;
            services.ConfigureOptions<ConfigureJwtProviderOptions>();
            services.ConfigureOptions<ConfigureJwtBearerOptions>();
            services.AddScoped<IJwtProvider, JwtProvider>();
            return authenticationBuilder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme);
        }
    }
}
