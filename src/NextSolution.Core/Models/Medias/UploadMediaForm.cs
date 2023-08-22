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
    public class UploadContentForm
    {
        public string FileName { get; set; } = default!;

        public long FileSize { get; set; } = default!;

        public Stream Content { get; set; } = default!;

        public string? ContentType { get; set; }

        public MediaType? MediaType { get; set; }

        public class Validator : AbstractValidator<UploadContentForm>
        {
            public Validator(IOptions<MediaServiceOptions> mediaServiceOptions)
            {
                RuleFor(_ => _.FileName)
                    .NotEmpty()
                    .Must(IsValidFileName).WithMessage("'{PropertyName}' contains invalid characters.")
                    .Must((form, fileName) => mediaServiceOptions.Value.HasMediaTypeInfo(fileName, form.MediaType)).WithMessage("'{PropertyName}' is not allowed.");

                RuleFor(_ => _.FileSize)
                    .GreaterThan(0)
                    .Must((form, fileSize) => fileSize > (mediaServiceOptions.Value.GetMediaTypeInfo(form.FileName, form.MediaType)?.FileSize ?? 0))
                    .WithMessage((form, fileSize) => $"'{{PropertyName}}' must be less then {(mediaServiceOptions.Value.GetMediaTypeInfo(form.FileName, form.MediaType)?.FileSize ?? 0).Kilobytes().Humanize()}.");

                RuleFor(_ => _.Content).NotNull();
            }

            private static bool IsValidFileName(string fileName)
            {
                char[] invalidChars = Path.GetInvalidFileNameChars();
                return !fileName.Any(c => invalidChars.Contains(c));
            }
        }
    }

    public class UploadChunkForm 
    {
        public string FileName { get; set; } = default!;

        public long FileSize { get; set; } = default!;

        public string? ContentType { get; set; }

        public MediaType? MediaType { get; set; }

        public Stream Chunk { get; set; } = default!;

        public long Offset { get; set; } 

        public class Validator : AbstractValidator<UploadChunkForm>
        {
            public Validator(IOptions<MediaServiceOptions> mediaServiceOptions)
            {
                RuleFor(_ => _.FileName)
                    .NotEmpty()
                    .Must(IsValidFileName).WithMessage("'{PropertyName}' contains invalid characters.")
                    .Must((form, fileName) => mediaServiceOptions.Value.HasMediaTypeInfo(fileName, form.MediaType)).WithMessage("'{PropertyName}' is not allowed.");

                RuleFor(_ => _.FileSize)
                    .GreaterThan(0)
                    .Must((form, fileSize) => fileSize > (mediaServiceOptions.Value.GetMediaTypeInfo(form.FileName, form.MediaType)?.FileSize ?? 0))
                    .WithMessage((form, fileSize) => $"'{{PropertyName}}' must be less then {(mediaServiceOptions.Value.GetMediaTypeInfo(form.FileName, form.MediaType)?.FileSize ?? 0).Kilobytes().Humanize()}.");

                RuleFor(_ => _.Chunk).NotNull();
            }

            private static bool IsValidFileName(string fileName)
            {
                char[] invalidChars = Path.GetInvalidFileNameChars();
                return !fileName.Any(c => invalidChars.Contains(c));
            }
        }
    }
}
