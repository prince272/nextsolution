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
    public class UploadMediaByFileContentForm : IUploadMediaForm
    {
        public string FileId { get; set; } = default!;

        public string FileName { get; set; } = default!;

        public long FileSize { get; set; } = default!;

        public MediaType? MediaType { get; set; }

        public Stream Content { get; set; } = default!;

        public string? ContentType { get; set; }

        public class Validator : UploadFormAbstractValidator<UploadMediaByFileContentForm>
        {
            public Validator(IOptions<MediaServiceOptions> mediaServiceOptions) : base(mediaServiceOptions)
            {
            }
        }
    }

    public interface IUploadMediaForm
    {
        string FileId { get; set; }

        string FileName { get; set; } 

        long FileSize { get; set; } 

        string? ContentType { get; set; }

        Stream Content { get; set; }

        MediaType? MediaType { get; set; }
    }

    public abstract class UploadFormAbstractValidator<TUploadMediaForm> : AbstractValidator<TUploadMediaForm>
        where TUploadMediaForm : class, IUploadMediaForm
    {
        public UploadFormAbstractValidator(IOptions<MediaServiceOptions> mediaServiceOptions)
        {
            RuleFor(_ => _.FileName)
                .NotEmpty()
                .Must(IsValidFileName).WithMessage("'{PropertyName}' contains invalid characters.")
                .Must((form, fileName) => mediaServiceOptions.Value.HasMediaTypeInfo(fileName, form.MediaType)).WithMessage("'{PropertyName}' is not allowed.");

            RuleFor(_ => _.FileSize)
                .Must((form, fileSize) =>
                {
                    var maxFileSize = (mediaServiceOptions.Value.GetMediaTypeInfo(form.FileName, form.MediaType)
                   ?? mediaServiceOptions.Value.All.OrderByDescending(_ => _.FileSize).First()).FileSize;

                    return fileSize <= maxFileSize;
                })
                .WithMessage((form, fileSize) =>
                {
                    var maxFileSize = (mediaServiceOptions.Value.GetMediaTypeInfo(form.FileName, form.MediaType)
                   ?? mediaServiceOptions.Value.All.OrderByDescending(_ => _.FileSize).First()).FileSize;

                    return $"'{{PropertyName}}' must be {maxFileSize.Bytes().Humanize()} or smaller.";
                });

            RuleFor(_ => _.Content).NotNull();
        }

        private static bool IsValidFileName(string fileName)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            return !fileName.Any(c => invalidChars.Contains(c));
        }
    }
}
