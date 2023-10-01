using NextSolution.Core.Entities;
using NextSolution.Core.Shared;

namespace NextSolution.Core.Repositories
{
    public interface IChatMessageRepository : IRepository<ChatMessage>
    {
        Task<IEnumerable<ChatMessage>> GetAncestorsAsync(ChatMessage message);
    }
}
