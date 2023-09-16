using Microsoft.Extensions.Options;
using NextSolution.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Models.Medias
{
    public class UploadMediaChunkForm : UploadMediaContentForm
    {
        public long Id { get; set; }

        public long Offset { get; set; }
    }

    public class UploadMediaChunkFormValidator : UploadMediaContentFormValidator<UploadMediaChunkForm>
    {
        public UploadMediaChunkFormValidator(IOptions<MediaServiceOptions> mediaServiceOptions) : base(mediaServiceOptions)
        {
        }
    }
}
