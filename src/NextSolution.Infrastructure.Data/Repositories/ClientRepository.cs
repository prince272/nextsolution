using Microsoft.EntityFrameworkCore;
using NextSolution.Core.Entities;
using NextSolution.Core.Repositories;
using System.Linq.Expressions;

namespace NextSolution.Infrastructure.Data.Repositories
{
    public class ClientRepository : AppRepository<Client>, IClientRepository
    {
        public ClientRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public Task DeactivateAllAsync(CancellationToken cancellationToken = default)
        {
            return _dbContext.Set<Client>().ExecuteUpdateAsync(calls => calls.SetProperty(client => client.Active, client => false), cancellationToken);
        }

        public Task DeactivateAsync(Client client, CancellationToken cancellationToken = default)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            client.Active = false;
            _dbContext.Update(client);
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task DeactivateManyAsync(Expression<Func<Client, bool>> predicate, CancellationToken cancellationToken = default)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return _dbContext.Set<Client>().Where(predicate).ExecuteUpdateAsync(calls => calls.SetProperty(client => client.Active, client => false), cancellationToken);
        }
    }
}