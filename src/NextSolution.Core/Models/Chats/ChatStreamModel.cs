using NextSolution.Core.Entities;

namespace NextSolution.Core.Models.Chats
{
    public class ChatStreamModel
    {
        public long? ChatId { get; set; }

        public string ChatTitle { get; set; } = default!;

        public ChatMessageModel User { get; set; } = default!;

        public ChatMessageModel Assistant { get; set; } = default!;
    }
}
