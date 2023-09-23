using NextSolution.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Entities
{
    public class ChatMessage : IEntity
    {
        public virtual Chat Chat { get; set; } = default!;

        public long ChatId { get; set; }

        public long Id { get; set; }

        public string Role { get; set; } = default!;

        public string Content { get; set; } = default!;

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }
}