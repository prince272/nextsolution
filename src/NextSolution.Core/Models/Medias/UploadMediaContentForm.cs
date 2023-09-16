using FluentValidation;
using Microsoft.Extensions.Options;
using NextSolution.Core.Entities;
using NextSolution.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Models.Medias
{
    public class UploadMediaContentForm
    {
        public string Name { get; set; } = default!;

        public string Path { get; set; } = default!;

        public long Size { get; set; } = default!;

        public MediaType? Type { get; set; }

        public Stream Content { get; set; } = default!;

        public string? ContentType { get; set; }
    }

    public class UploadMediaContentFormValidator : UploadMediaContentFormValidator<UploadMediaContentForm>
    {
        public UploadMediaContentFormValidator(IOptions<MediaServiceOptions> mediaServiceOptions) : base(mediaServiceOptions)
        {
        }
    }

    public abstract class UploadMediaContentFormValidator<TUploadMediaContentForm> : AbstractValidator<TUploadMediaContentForm> where TUploadMediaContentForm : UploadMediaContentForm
    {
        public UploadMediaContentFormValidator(IOptions<MediaServiceOptions> mediaServiceOptions)
        {
            RuleFor(_ => _.Path)
                .NotEmpty()
                .Must(IsValidPath).WithMessage("File name contains invalid characters.")
                .Must((form, path) => mediaServiceOptions.Value.HasMediaTypeInfo(path, form.Type)).WithMessage("File not allowed.");

            RuleFor(_ => _.Size)
                .Must((form, fileSize) =>
                {
                    var maxFileSize = (mediaServiceOptions.Value.GetMediaTypeInfo(form.Path, form.Type)
                   ?? mediaServiceOptions.Value.All.OrderByDescending(_ => _.FileSize).First()).FileSize;

                    return fileSize <= maxFileSize;
                })
                .WithMessage((form, fileSize) =>
                {
                    var maxFileSize = (mediaServiceOptions.Value.GetMediaTypeInfo(form.Path, form.Type)
                   ?? mediaServiceOptions.Value.All.OrderByDescending(_ => _.FileSize).First()).FileSize;

                    return $"File size must be {maxFileSize.Bytes().Humanize()} or smaller.";
                });

            RuleFor(_ => _.Content).NotNull();
        }

        private static bool IsValidPath(string path)
        {
            var fileName = Path.GetFileName(path);

            var invalidFileNameChars = fileName.Where(c => Path.GetInvalidPathChars().Concat(new[] { '/', '\\' }).Contains(c)).ToArray();
            if (invalidFileNameChars.Length > 0) return false;

            var directoryNames = Path.GetDirectoryName(path)?.Split(new char[] { '/', '\\' }) ?? Array.Empty<string>();

            foreach (var directoryName in directoryNames)
            {
                var invalidDirectoryNameChars = directoryName.Where(c => Path.GetInvalidPathChars().Concat(new[] { '/', '\\' }).Contains(c)).ToArray();
                if (invalidDirectoryNameChars.Length > 0) return false;
            }

            return true;
        }
    }
}
