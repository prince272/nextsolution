using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace NextSolution.WebApi.Shared
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDocumentations(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                var assembly = Assembly.GetExecutingAssembly();
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = assembly.GetName().Name,
                    Description = "An ASP.NET Core Web API for managing ToDo items",
                    TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "Example Contact",
                        Url = new Uri("https://example.com/contact")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Example License",
                        Url = new Uri("https://example.com/license")
                    }
                });

                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "\"Standard Authorization header using the Bearer scheme (JWT). Example: \\\"Bearer {token}\\\"\"",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = JwtBearerDefaults.AuthenticationScheme
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            }, Array.Empty<string>()
        }
        });

                var xmlFilePath = Path.Combine(AppContext.BaseDirectory, $"{assembly.GetName().Name}.xml");
                if (File.Exists(xmlFilePath)) options.IncludeXmlComments(xmlFilePath);
            });
            return services;
        }
    }
}