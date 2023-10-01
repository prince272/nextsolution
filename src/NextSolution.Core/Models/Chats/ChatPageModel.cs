namespace NextSolution.Core.Models.Chats
{
    public class ChatPageModel : ChatListModel
    {
        public long Offset { get; set; }

        public int Limit { get; set; }

        public long Length { get; set; }

        public long? Previous { get; set; }

        public long? Next { get; set; }
    }
}
