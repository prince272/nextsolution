using NextSolution.Core.Entities;
using NextSolution.Core.Shared;
using System.Linq.Expressions;

namespace NextSolution.Core.Repositories
{
    public interface IClientRepository : IRepository<Client>
    {
        Task DeactivateAsync(Client client, CancellationToken cancellationToken = default);

        Task DeactivateManyAsync(Expression<Func<Client, bool>> predicate, CancellationToken cancellationToken = default);

        Task DeactivateAllAsync(CancellationToken cancellationToken = default);
    }
}
