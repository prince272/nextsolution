using Microsoft.Extensions.DependencyInjection;
using NextSolution.Core.Entities;
using NextSolution.Core.Extensions.FileStorage;
using NextSolution.Core.Utilities;

namespace NextSolution.Infrastructure.FileStorage.Local
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLocalFileStorage(this IServiceCollection services, Action<LocalFileStorageOptions> options)
        {
            services.AddFileTypeOptions();
            services.Configure(options);
            services.AddLocalFileStorage();
            return services;
        }

        public static IServiceCollection AddLocalFileStorage(this IServiceCollection services)
        {
            services.AddFileTypeOptions();
            services.AddTransient<IFileStorage, LocalFileStorage>();
            return services;
        }

        public static IServiceCollection AddFileTypeOptions(this IServiceCollection services)
        {
            services.Configure<FileRuleOptions>(options =>
            {
                options.Documents = new[] { ".doc", ".docx", ".rtf", ".pdf" }.Select(fileExtension =>
                {

                    return new FileRule
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

                    return new FileRule
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

                    return new FileRule
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

                    return new FileRule
                    {
                        MediaType = MediaType.Audio,
                        FileExtension = fileExtension,
                        FileSize = 83886080L,
                        ContentType = MimeTypes.GetMimeType(fileExtension)
                    };
                })
                .ToArray(); // Audio - 80MB
            });
            return services;
        }
    }
}
