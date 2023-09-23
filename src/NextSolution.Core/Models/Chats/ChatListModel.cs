using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Models.Chats
{
    public class ChatListModel
    {
        public IEnumerable<ChatModel> Items { get; set; } = new List<ChatModel>();
    }
}
