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
    public class ChatRepository : AppRepository<Chat>, IChatRepository
    {
        public ChatRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        protected override IQueryable<Chat> GetQueryable(
            Expression<Func<Chat, bool>>? predicate = null,
            Func<IQueryable<Chat>, IOrderedQueryable<Chat>>? orderBy = null,
            Expression<Func<Chat, object>>[]? include = null,
            bool enableTracking = true,
            bool enableFilters = false)
        {
            var query = base.GetQueryable(predicate, orderBy, include, enableTracking, enableFilters);
            query = query.OrderByDescending(_ => _.UpdatedAt);
            return query;
        }
    }
}
