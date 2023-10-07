using Microsoft.EntityFrameworkCore;
using NextSolution.Core.Entities;
using NextSolution.Core.Repositories;
using System.Linq.Expressions;
using System.Threading;

namespace NextSolution.Infrastructure.Data.Repositories
{
    public class ChatMessageRepository : AppRepository<ChatMessage>, IChatMessageRepository
    {
        public ChatMessageRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}