using NextSolution.Core.Entities;
using NextSolution.Core.Shared;

namespace NextSolution.Core.Repositories
{
    public interface IRoleRepository : IRepository<Role>
    {
        Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    }
}
