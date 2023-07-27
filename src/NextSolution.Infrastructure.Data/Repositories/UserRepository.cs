using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NextSolution.Core.Entities;
using NextSolution.Core.Utilities;
using NextSolution.Core.Repositories;
using NextSolution.Infrastructure.Data;
using NextSolution.Infrastructure.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Infrastructure.Data.Repositories
{
    public class UserRepository : AppRepository<User>, IUserRepository
    {
        private readonly UserManager<User> _userManager;

        public UserRepository(UserManager<User> userManager, AppDbContext dbContext) : base(dbContext)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task CreateAsync(User user, string password)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            ArgumentException.ThrowIfNullOrEmpty(password?.Trim(), nameof(password));

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded) throw new InvalidOperationException(result.Errors.GetMessage());
        }

        public override async Task CreateAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var result = await _userManager.CreateAsync(user);

            if (!result.Succeeded) throw new InvalidOperationException(result.Errors.GetMessage());
        }

        public override async Task UpdateAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded) throw new InvalidOperationException(result.Errors.GetMessage());
        }

        public override async Task DeleteAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded) throw new InvalidOperationException(result.Errors.GetMessage());
        }

        public Task<User?> FindByEmailAsync(string email)
        {
            if (email == null) throw new ArgumentNullException(nameof(email));

            return _userManager.FindByEmailAsync(email);
        }

        // TODO: Improve comparing phone numbers
        public Task<User?> FindByPhoneNumberAsync(string phoneNumber)
        {
            return _userManager.Users.FirstOrDefaultAsync(_ => _.PhoneNumber == phoneNumber);
        }

        public async Task AddToRoleAsync(User user, string role)
        {
            var result = await _userManager.AddToRoleAsync(user, role);

            if (!result.Succeeded) throw new InvalidOperationException(result.Errors.GetMessage());
        }

        public async Task AddToRolesAsync(User user, IEnumerable<string> roles)
        {
            var result = await _userManager.AddToRolesAsync(user, roles);

            if (!result.Succeeded) throw new InvalidOperationException(result.Errors.GetMessage());
        }

        public async Task GenerateUserNameAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            user.UserName = await AlgorithmHelper.GenerateSlugAsync($"{user.FirstName} {user.LastName}".ToLowerInvariant(), userName => _userManager.Users.AnyAsync(_ => _.UserName == userName));
        }
    }
}