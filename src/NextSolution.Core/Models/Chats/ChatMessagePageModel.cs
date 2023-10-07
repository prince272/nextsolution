namespace NextSolution.Core.Models.Chats
{
    public class ChatMessagePageModel
    {
        public long Offset { get; set; }

        public int Limit { get; set; }

        public long Length { get; set; }

        public long? Previous { get; set; }

        public long? Next { get; set; }

        public IList<ChatMessageModel> Items { get; set; } = new List<ChatMessageModel>();
    }
}
