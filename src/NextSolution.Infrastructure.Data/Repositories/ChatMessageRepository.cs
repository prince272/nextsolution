using NextSolution.Core.Entities;
using NextSolution.Core.Repositories;
using System.Linq.Expressions;

namespace NextSolution.Infrastructure.Data.Repositories
{
    public class ChatMessageRepository : AppRepository<ChatMessage>, IChatMessageRepository
    {
        public ChatMessageRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        //public async Task<IEnumerable<ChatMessage>> GetAdjacentSiblingsAsync(ChatMessage message)
        //{
        //    var chatId = message.ChatId;
        //    var parentId = message.ParentId;
        //    var messageId = message.Id;

        //    var siblings = await _dbContext.Set<ChatMessage>()
        //        .Where(m => m.ChatId == chatId && m.ParentId == parentId && m.Id != messageId)
        //        .ToListAsync();

        //    return siblings;
        //}

        public async Task<IEnumerable<ChatMessage>> GetAncestorsAsync(ChatMessage message)
        {
            var ancestors = new List<ChatMessage>();

            while (message?.PreviousId != null)
            {
                message = (await _dbContext.Set<ChatMessage>().FindAsync(message.PreviousId))!;
                if (message != null) ancestors.Add(message);
            }

            return ancestors;
        }


        protected override IQueryable<ChatMessage> GetQuery(
                Expression<Func<ChatMessage, bool>>? predicate = null,
                Func<IQueryable<ChatMessage>, IOrderedQueryable<ChatMessage>>? orderBy = null,
                Expression<Func<ChatMessage, object>>[]? include = null,
                bool enableTracking = true,
                bool enableFilters = false)
        {
            var query = base.GetQuery(predicate, orderBy, include, enableTracking, enableFilters);
            query = query.OrderByDescending(_ => _.UpdatedAt);
            return query;
        }
    }
}