using AbstractProfile = AutoMapper.Profile;
using NextSolution.Core.Entities;
using NextSolution.Core.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Models.Medias
{
    public class MediaModel
    {
        public long Id { get; set; }

        public string Path { get; set; } = default!;

        public string Name { get; set; } = default!;

        public long Size { get; set; }

        public string Url { get; set; } = default!;

        public string ContentType { get; set; } = default!;

        public Stream Content { get; set; } = default!;

        public MediaType Type { get; set; }

        public int? Width { get; set; }

        public int? Height { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }

    public class MediaModelProfile : AbstractProfile
    {
        public MediaModelProfile()
        {
            CreateMap<Media, MediaModel>();
        }
    }
}
