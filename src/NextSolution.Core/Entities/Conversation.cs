using NextSolution.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Entities
{
    public class Conversation : IEntity
    {
        public long Id { get; set; }

        public string Title { get; set; } = default!;

        public ConversationType Type { get; set; }

        public virtual ICollection<UserConversation> Users { get; set; } = new List<UserConversation>(); 

        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }

    public class UserConversation : IEntity
    {
        public long Id { get; set; }

        public virtual User User { get; set; } = default!;

        public long UserId { get; set; }

        public virtual Conversation Conversation { get; set; } = default!;

        public long ConversationId { get; set; }
    }

    public enum ConversationType
    {
        Private,
        Group
    }
}