using NextSolution.Core.Entities;
using AbstractProfile = AutoMapper.Profile;

namespace NextSolution.Core.Models.Chats
{
    public class ChatMessageModel
    {
        public long ChatId { get; set; }

        public long? ParentId { get; set; }

        public long Id { get; set; }

        public ChatMessageRole Role { get; set; } = default!;

        public string Content { get; set; } = default!;

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }

    public class ChatMessageModelProfile : AbstractProfile
    {
        public ChatMessageModelProfile()
        {
            CreateMap<ChatMessage, ChatMessageModel>();
        }
    }
}
