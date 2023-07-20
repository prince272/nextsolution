using NextSolution.Core.Entities;
using NextSolution.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task CreateAsync(User user, string password, CancellationToken cancellationToken = default);
    }
}
