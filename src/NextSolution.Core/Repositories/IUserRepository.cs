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
        Task CreateAsync(User user, string password);

        Task<User?> FindByEmailAsync(string email);

        Task<User?> FindByPhoneNumberAsync(string phoneNumber);

        Task AddToRoleAsync(User user, string role);

        Task AddToRolesAsync(User user, IEnumerable<string> roles);

        Task GenerateUserNameAsync(User user);
    }
}
