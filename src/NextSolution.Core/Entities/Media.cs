using NextSolution.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Entities
{
    public class Media : IEntity
    {
        public long Id { get; set; }

        public string FileId { get; set; } = default!;

        public string FileName { get; set; } = default!;

        public long FileSize { get; set; }

        public string ContentType { get; set; } = default!;

        public MediaType MediaType { get; set; }

        public int? Width { get; set; }

        public int? Height { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }

    public enum MediaType
    {
        Document,
        Image,
        Video,
        Audio,
        Unknown
    }
}
