using Microsoft.AspNetCore.Mvc;

namespace NextSolution.WebApi.Models
{

    public class FileChunkForm
    {
        [FromHeader(Name = "Upload-Name")]
        public string FileName { get; set; } = default!;

        [FromHeader(Name = "Upload-Length")]
        public long FileSize { get; set; }

        [FromHeader(Name = "Upload-Type")]
        public string ContentType { get; set; } = default!;

        [FromHeader(Name = "Upload-Offset")]
        public long Offset { get; set; }
    }
}
