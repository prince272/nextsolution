using NextSolution.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Entities
{
    public class Message : IEntity
    {
        public long Id { get; set; }

        public virtual User Sender { get; set; } = default!;

        public long SenderId { get; set; }

        public virtual Conversation Conversation { get; set; } = default!;

        public long ConversationId { get; set; }

        public virtual Media? Media { get; set; } = default!;

        public long? MediaId { get; set; }

        public string Content { get; set; } = default!;

        public MessageStatus Status { get; set; }

        public DateTimeOffset SentAt { get; set; }

        public DateTimeOffset DeliveredAt { get; set; }

        public DateTimeOffset ReadAt { get; set; }
    }

    public enum MessageStatus
    {
        Sent,
        Delivered,
        Read
    }
}
