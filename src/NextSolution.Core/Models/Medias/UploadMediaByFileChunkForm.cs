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
    public class UploadMediaByFileChunkForm : IUploadMediaForm
    {
        public string FileId { get; set; } = default!;

        public string FileName { get; set; } = default!;

        public long FileSize { get; set; } = default!;

        public MediaType? MediaType { get; set; }

        public Stream Content { get; set; } = default!;

        public string? ContentType { get; set; }

        public long Offset { get; set; }

        public class Validator : UploadFormAbstractValidator<UploadMediaByFileChunkForm>
        {
            public Validator(IOptions<MediaServiceOptions> mediaServiceOptions) : base(mediaServiceOptions)
            {
            }
        }
    }
}
