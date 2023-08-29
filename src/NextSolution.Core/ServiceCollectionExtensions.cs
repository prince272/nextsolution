using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NextSolution.Core.Utilities;
using NextSolution.Core.Services;
using NextSolution.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NextSolution.Core.Entities;
using FluentValidation;

namespace NextSolution.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IAccountService, AccountService>();

            services.AddScoped<IUserService, UserService>();

            services.Configure<MediaServiceOptions>(options =>
            {
                options.Documents = new[] { ".doc", ".docx", ".rtf", ".pdf" }.Select(fileExtension =>
                {

                    return new MediaTypeInfo
                    {
                        MediaType = MediaType.Document,
                        FileExtension = fileExtension,
                        FileSize = 83886080L,
                        ContentType = MimeTypes.GetMimeType(fileExtension)
                    };
                })
                .ToArray(); // Document - 80MB

                options.Images = new[] { ".jpg", ".jpeg", ".png" }.Select(fileExtension =>
                {

                    return new MediaTypeInfo
                    {
                        MediaType = MediaType.Image,
                        FileExtension = fileExtension,
                        FileSize = 5242880L,
                        ContentType = MimeTypes.GetMimeType(fileExtension)
                    };
                })
                .ToArray(); // Image - 5MB

                options.Videos = new[] { ".mp4", ".webm", ".swf", ".flv" }.Select(fileExtension =>
                {

                    return new MediaTypeInfo
                    {
                        MediaType = MediaType.Video,
                        FileExtension = fileExtension,
                        FileSize = 524288000L,
                        ContentType = MimeTypes.GetMimeType(fileExtension)
                    };
                })
                .ToArray(); // Video - 500MB

                options.Audios = new[] { ".mp3", ".ogg", ".wav" }.Select(fileExtension =>
                {

                    return new MediaTypeInfo
                    {
                        MediaType = MediaType.Audio,
                        FileExtension = fileExtension,
                        FileSize = 83886080L,
                        ContentType = MimeTypes.GetMimeType(fileExtension)
                    };
                })
                .ToArray(); // Audio - 80MB
            });

            services.AddScoped<IMediaService, MediaService>();
            return services;
        }

        public static IServiceCollection AddValidators(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Continue;
            ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
            ValidatorOptions.Global.DisplayNameResolver = (type, memberInfo, expression) =>
            {
                string? RelovePropertyName()
                {
                    if (expression != null)
                    {
                        var chain = FluentValidation.Internal.PropertyChain.FromExpression(expression);
                        if (chain.Count > 0) return chain.ToString();
                    }

                    if (memberInfo != null)
                    {
                        return memberInfo.Name;
                    }

                    return null;
                }

                return RelovePropertyName()?.Humanize();
            };

            var validatorTypes = assemblies.SelectMany(_ => _.DefinedTypes).Select(_ => _.AsType()).Where(type => type.IsClass && !type.IsAbstract && !type.IsGenericType && type.IsCompatibleWith(typeof(IValidator<>)));

            foreach (var concreteType in validatorTypes)
            {
                var matchingInterfaceType = concreteType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>));

                if (matchingInterfaceType != null)
                {
                    services.AddScoped(concreteType);
                }
            }

            return services;
        }
    }
}