using Microsoft.AspNetCore.Identity;
using NextSolution.Core.Entities;
using NextSolution.Core.Repositories;
using NextSolution.Core.Shared;
using NextSolution.Infrastructure.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Infrastructure.Data.Repositories
{
    public class RoleRepository : AppRepository<Role>, IRoleRepository
    {
        private readonly RoleManager<Role> _roleManager;

        public RoleRepository(RoleManager<Role> roleManager, AppDbContext dbContext) : base(dbContext)
        {
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        public override async Task<Role> CreateAsync(Role role, CancellationToken cancellationToken = default)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded) throw new InvalidOperationException(result.Errors.GetMessage());
            return role;
        }

        public override async Task<Role> UpdateAsync(Role role, CancellationToken cancellationToken = default)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded) throw new InvalidOperationException(result.Errors.GetMessage());
            return role;
        }

        public override async Task DeleteAsync(Role role, CancellationToken cancellationToken = default)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));

            var result = await _roleManager.DeleteAsync(role);

            if (!result.Succeeded) throw new InvalidOperationException(result.Errors.GetMessage());
        }

        public Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            return _roleManager.FindByNameAsync(name);
        }
    }
}
