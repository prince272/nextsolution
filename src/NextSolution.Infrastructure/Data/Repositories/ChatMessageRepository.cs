using NextSolution.Core.Entities;
using NextSolution.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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