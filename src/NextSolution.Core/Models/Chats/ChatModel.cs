using AbstractProfile = AutoMapper.Profile;
using NextSolution.Core.Entities;
using NextSolution.Core.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Models.Chats
{
    public class ChatModel
    {
        public long UserId { get; set; }

        public long Id { get; set; }

        public string Title { get; set; } = default!;

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }

    public class ChatModelProfile : AbstractProfile
    {
        public ChatModelProfile()
        {
            CreateMap<Chat, ChatModel>();
        }
    }
}
