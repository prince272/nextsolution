using NextSolution.Core.Entities;
using NextSolution.Core.Utilities;
using System.Linq.Expressions;

namespace NextSolution.Core.Models.Users
{
    public class UserSearchCriteria
    {
        public long[]? Id { get; set; }

        public bool? Online { get; set; }

        public Expression<Func<User, bool>> Build()
        {
            var predicate = PredicateBuilder.True<User>();

            if (Id != null && Id.Any())
            {
                predicate = predicate.And(user => Id.Contains(user.Id));
            }

            if (Online.HasValue)
            {
                if (Online.Value)
                    predicate = predicate.And(user => user.Clients.Any(_ => _.Active));
                else
                    predicate = predicate.And(user => !user.Clients.Any(_ => _.Active));
            }

            return predicate;
        }
    }
}
