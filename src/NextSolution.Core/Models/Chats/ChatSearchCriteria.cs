using NextSolution.Core.Entities;
using NextSolution.Core.Utilities;
using System.Linq.Expressions;

namespace NextSolution.Core.Models.Chats
{
    public class ChatSearchCriteria
    {
        public long[]? Id { get; set; }

        public Expression<Func<Chat, bool>> Build()
        {
            var predicate = PredicateBuilder.True<Chat>();

            if (Id != null && Id.Any())
            {
                predicate = predicate.And(user => Id.Contains(user.Id));
            }

            return predicate;
        }
    }
}
