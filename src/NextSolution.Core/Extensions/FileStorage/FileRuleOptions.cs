using NextSolution.Core.Entities;
using NextSolution.Core.Utilities;

namespace NextSolution.Core.Extensions.FileStorage
{
    public class FileRuleOptions
    {
        public IList<FileRule> Documents { get; set; } = new List<FileRule>();

        public IList<FileRule> Images { get; set; } = new List<FileRule>();

        public IList<FileRule> Videos { get; set; } = new List<FileRule>();

        public IList<FileRule> Audios { get; set; } = new List<FileRule>();

        public IEnumerable<FileRule> All => new[] { Documents, Images, Videos, Audios }.SelectMany(_ => _).ToArray();

        public bool HasFileRule(string fileName, MediaType? mediaType = null)
        {
            var fileExtension = Path.GetExtension(fileName);
            var fileRules = mediaType switch
            {
                MediaType.Document => Documents,
                MediaType.Image => Images,
                MediaType.Video => Videos,
                MediaType.Audio => Audios,
                _ => All,
            };
            var result = fileRules.Any(_ => _.FileExtension.Equals(fileExtension, StringComparison.OrdinalIgnoreCase));
            return result;
        }

        public FileRule? GetFileRule(string fileName, MediaType? mediaType = null)
        {
            var fileExtension = Path.GetExtension(fileName);
            var fileRules = mediaType switch
            {
                MediaType.Document => Documents,
                MediaType.Image => Images,
                MediaType.Video => Videos,
                MediaType.Audio => Audios,
                _ => All,
            };
            return fileRules.FirstOrDefault(_ => _.FileExtension.Equals(fileExtension, StringComparison.OrdinalIgnoreCase));
        }

        public MediaType GetMediaType(string fileName, MediaType? mediaType = null)
        {
            FileRule? fileRule = GetFileRule(fileName, mediaType);
            return fileRule?.MediaType ?? MediaType.Unknown;
        }

        public string GetContentType(string fileName, MediaType? mediaType = null)
        {
            FileRule? fileRule = GetFileRule(fileName, mediaType);
            return fileRule?.ContentType ?? MimeTypes.FallbackMimeType;
        }
    }

    public class FileRule
    {
        public string ContentType { get; set; } = default!;

        public MediaType MediaType { get; set; }

        public long FileSize { get; set; }

        public string FileExtension { get; set; } = default!;
    }
}
