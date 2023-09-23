using NextSolution.Core.Entities;
using NextSolution.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Models.Chats
{
    public class ChatSearchParams
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
