using NextSolution.Server.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace NextSolution.Server.Providers.SwaggerGen
{
    public class ConfigureSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ConfigureSwaggerGenOptions(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public void Configure(SwaggerGenOptions options)
        {
            var httpContext = (_httpContextAccessor?.HttpContext) ?? throw new InvalidOperationException("Unable to determine the current HttpContext.");
            string currentOrigin = string.Concat(httpContext.Request.Scheme, "://", httpContext.Request.Host.ToUriComponent()).ToLower();

            var assembly = Assembly.GetExecutingAssembly();
            var assemblyInfo = new AssemblyInfo(assembly);

            var jwtSecurityScheme = new OpenApiSecurityScheme
            {
                BearerFormat = "JWT",
                Name = "JWT Authentication",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };

            options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { jwtSecurityScheme, Array.Empty<string>() }
            });

            options.AddServer(new OpenApiServer
            {
                Url = currentOrigin,
                Description = "Defines the server URL and configuration for API operations.",
                Variables = new Dictionary<string, OpenApiServerVariable>
                {
                    {
                        "baseUrl", new OpenApiServerVariable { Default = currentOrigin, Description = "Base URL of the target server." }
                    }
                }
            });

            var xmlFilePath = Path.Combine(AppContext.BaseDirectory, $"{assembly.GetName().Name}.xml");
            if (File.Exists(xmlFilePath)) options.IncludeXmlComments(xmlFilePath);
        }
    }
}
