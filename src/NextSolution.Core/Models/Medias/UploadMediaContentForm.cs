using FluentValidation;
using Microsoft.Extensions.Options;
using NextSolution.Core.Entities;
using NextSolution.Core.Extensions.FileStorage;
using NextSolution.Core.Services;
using NextSolution.Core.Utilities;
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
        public UploadMediaContentFormValidator(IOptions<FileRuleOptions> fileTypeOptions) : base(fileTypeOptions)
        {
        }
    }

    public abstract class UploadMediaContentFormValidator<TUploadMediaContentForm> : AbstractValidator<TUploadMediaContentForm> where TUploadMediaContentForm : UploadMediaContentForm
    {
        public UploadMediaContentFormValidator(IOptions<FileRuleOptions> fileTypeOptions)
        {
            RuleFor(_ => _.Path)
                .NotEmpty()
                .Must(ValidationHelper.IsValidPath).WithMessage("File name contains invalid characters.")
                .Must((form, path) => fileTypeOptions.Value.HasFileRule(path, form.Type)).WithMessage("File not allowed.");

            RuleFor(_ => _.Size)
                .Must((form, fileSize) =>
                {
                    var maxFileSize = (fileTypeOptions.Value.GetFileRule(form.Path, form.Type)
                   ?? fileTypeOptions.Value.All.OrderByDescending(_ => _.FileSize).First()).FileSize;

                    return fileSize <= maxFileSize;
                })
                .WithMessage((form, fileSize) =>
                {
                    var maxFileSize = (fileTypeOptions.Value.GetFileRule(form.Path, form.Type)
                   ?? fileTypeOptions.Value.All.OrderByDescending(_ => _.FileSize).First()).FileSize;

                    return $"File size must be {maxFileSize.Bytes().Humanize()} or smaller.";
                });

            RuleFor(_ => _.Content).NotNull();
        }
    }
}
