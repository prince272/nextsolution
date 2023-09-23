using NextSolution.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Entities
{
    public class Chat : IEntity
    {
        public virtual User User { get; set; } = default!;

        public long UserId { get; set; }

        public long Id { get; set; }

        public string Title { get; set; } = default!;

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }
}
