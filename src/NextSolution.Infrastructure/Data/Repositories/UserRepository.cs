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
using NextSolution.Core.Constants;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using NextSolution.Core.Extensions.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace NextSolution.Infrastructure.Data.Repositories
{
    public class UserRepository : AppRepository<User>, IUserRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserSessionFactory _userSessionFactory;
        private readonly IUserSessionStore _userSessionStore;

        public UserRepository(UserManager<User> userManager, AppDbContext dbContext, IUserSessionFactory userSessionFactory, IUserSessionStore userSessionStore) : base(dbContext)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _userSessionFactory = userSessionFactory ?? throw new ArgumentNullException(nameof(userSessionFactory));
            _userSessionStore = userSessionStore ?? throw new ArgumentNullException(nameof(userSessionStore));
        }

        public async Task CreateAsync(User user, string password, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (password == null) throw new ArgumentNullException(nameof(password));

            user.PhoneNumber = NormalizePhoneNumber(user.PhoneNumber);
            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded) throw new InvalidOperationException(result.Errors.GetMessage());
        }

        public override async Task CreateAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            user.PhoneNumber = NormalizePhoneNumber(user.PhoneNumber);
            var result = await _userManager.CreateAsync(user);

            if (!result.Succeeded) throw new InvalidOperationException(result.Errors.GetMessage());
        }

        public override async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            user.PhoneNumber = NormalizePhoneNumber(user.PhoneNumber);
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded) throw new InvalidOperationException(result.Errors.GetMessage());
        }

        public override async Task DeleteAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded) throw new InvalidOperationException(result.Errors.GetMessage());
        }

        public async Task UpdateLastActiveAsync(User user, CancellationToken cancellationToken = default)
        {
            var currentTime = DateTimeOffset.UtcNow;

            // Update the LastActive of a user only if at least 1 minute has elapsed since the last update.
            var threshold = TimeSpan.FromMinutes(1);
            if (user.Active && currentTime - user.LastActiveAt >= threshold)
            {
                user.LastActiveAt = currentTime;
                await _userManager.UpdateAsync(user);
            }
        }

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (email == null) throw new ArgumentNullException(nameof(email));

            return _userManager.FindByEmailAsync(email);
        }

        public Task<User?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            if (phoneNumber == null) throw new ArgumentNullException(nameof(phoneNumber));

            phoneNumber = NormalizePhoneNumber(phoneNumber);
            return _userManager.Users.FirstOrDefaultAsync(_ => _.PhoneNumber == phoneNumber, cancellationToken);
        }

        public Task<bool> IsInRoleAsync(User user, string role, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (role == null) throw new ArgumentNullException(nameof(role));

            return _userManager.IsInRoleAsync(user, role);
        }

        public async Task AddToRoleAsync(User user, string role, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (role == null) throw new ArgumentNullException(nameof(role));

            var result = await _userManager.AddToRoleAsync(user, role);

            if (!result.Succeeded) throw new InvalidOperationException(result.Errors.GetMessage());
        }

        public async Task AddToRolesAsync(User user, IEnumerable<string> roles, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (roles == null) throw new ArgumentNullException(nameof(roles));

            var result = await _userManager.AddToRolesAsync(user, roles);

            if (!result.Succeeded) throw new InvalidOperationException(result.Errors.GetMessage());
        }

        public async Task RemoveFromRoleAsync(User user, string role, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (role == null) throw new ArgumentNullException(nameof(role));

            var result = await _userManager.RemoveFromRoleAsync(user, role);

            if (!result.Succeeded) throw new InvalidOperationException(result.Errors.GetMessage());
        }

        public async Task RemoveFromRolesAsync(User user, IEnumerable<string> roles, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (roles == null) throw new ArgumentNullException(nameof(roles));

            var result = await _userManager.RemoveFromRolesAsync(user, roles);

            if (!result.Succeeded) throw new InvalidOperationException(result.Errors.GetMessage());
        }

        public async Task<IEnumerable<string>> GetRolesAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            return await _userManager.GetRolesAsync(user);
        }

        public Task<bool> CheckPasswordAsync(User user, string password, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (password == null) throw new ArgumentNullException(nameof(password));

            return _userManager.CheckPasswordAsync(user, password);
        }

        public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            return _userManager.HasPasswordAsync(user);
        }

        public async Task AddPasswordAsync(User user, string password, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (password == null) throw new ArgumentNullException(nameof(password));

            var result = await _userManager.AddPasswordAsync(user, password);

            if (!result.Succeeded) throw new InvalidOperationException(result.Errors.GetMessage());
        }

        public async Task ChangePasswordAsync(User user, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (currentPassword == null) throw new ArgumentNullException(nameof(currentPassword));
            if (newPassword == null) throw new ArgumentNullException(nameof(newPassword));

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (!result.Succeeded) throw new InvalidOperationException(result.Errors.GetMessage());
        }

        public async Task RemovePasswordAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var result = await _userManager.RemovePasswordAsync(user);

            if (!result.Succeeded) throw new InvalidOperationException(result.Errors.GetMessage());
        }

        public Task<string> GenerateEmailTokenAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            return _userManager.GenerateChangeEmailTokenAsync(user, user.Email!);
        }

        public async Task VerifyEmailAsync(User user, string token, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (token == null) throw new ArgumentNullException(nameof(token));

            var result = await _userManager.ChangeEmailAsync(user, user.Email!, token);
            if (!result.Succeeded) throw new InvalidOperationException(result.Errors.GetMessage());
        }

        public Task<string> GenerateChangeEmailTokenAsync(User user, string newEmail, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (newEmail == null) throw new ArgumentNullException(nameof(newEmail));

            return _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
        }

        public async Task ChangeEmailAsync(User user, string newEmail, string token, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (newEmail == null) throw new ArgumentNullException(nameof(newEmail));
            if (token == null) throw new ArgumentNullException(nameof(token));

            var result = await _userManager.ChangeEmailAsync(user, newEmail, token);
            if (!result.Succeeded) throw new InvalidOperationException(result.Errors.GetMessage());
        }

        public Task<string> GeneratePhoneNumberTokenAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            return _userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber!);
        }

        public async Task VerifyPhoneNumberTokenAsync(User user, string token, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (token == null) throw new ArgumentNullException(nameof(token));

            var result = await _userManager.ChangePhoneNumberAsync(user, user.PhoneNumber!, token);
            if (!result.Succeeded) throw new InvalidOperationException(result.Errors.GetMessage());
        }

        public Task<string> GenerateChangePhoneNumberTokenAsync(User user, string newPhoneNumber, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            return _userManager.GenerateChangePhoneNumberTokenAsync(user, newPhoneNumber);
        }

        public async Task ChangePhoneNumberAsync(User user, string newPhoneNumber, string token, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (newPhoneNumber == null) throw new ArgumentNullException(nameof(newPhoneNumber));
            if (token == null) throw new ArgumentNullException(nameof(token));

            var result = await _userManager.ChangePhoneNumberAsync(user, newPhoneNumber, token);
            if (!result.Succeeded) throw new InvalidOperationException(result.Errors.GetMessage());
        }

        public async Task<string> GeneratePasswordResetTokenAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return token;
        }

        public async Task ResetPasswordAsync(User user, string newPassword, string token, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded) throw new InvalidOperationException(result.Errors.GetMessage());
        }

        public Task<UserSessionInfo> GenerateSessionAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            return _userSessionFactory.GenerateAsync(user);
        }

        public Task AddSessionAsync(User user, UserSessionInfo session, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            return _userSessionStore.AddSessionAsync(user, session);
        }

        public Task RemoveSessionAsync(User user, string token, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            return _userSessionStore.RemoveSessionAsync(user, token);
        }

        public Task AddLoginAsync(User user, UserLoginInfo login, CancellationToken cancellationToken = default)
        {
            return _userManager.AddLoginAsync(user, login);
        }

        public Task RemoveLoginAsync(User user, string providerName, string providerKey, CancellationToken cancellationToken = default)
        {
            return _userManager.RemoveLoginAsync(user, providerName, providerKey);
        }


        public Task<User?> GetByAccessTokenAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            if (accessToken == null) throw new ArgumentNullException(nameof(accessToken));

            return _userSessionStore.GetUserByAccessTokenAsync(accessToken, cancellationToken);
        }

        public Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            if (refreshToken == null) throw new ArgumentNullException(nameof(refreshToken));

            return _userSessionStore.GetUserByRefreshTokenAsync(refreshToken, cancellationToken);
        }

        public Task<bool> ValidateAccessTokenAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            if (accessToken == null) throw new ArgumentNullException(nameof(accessToken));
            return _userSessionFactory.ValidateAccessTokenAsync(accessToken, cancellationToken);
        }

        public Task<bool> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            if (refreshToken == null) throw new ArgumentNullException(nameof(refreshToken));
            return _userSessionFactory.ValidateRefreshTokenAsync(refreshToken, cancellationToken);
        }

        public string? GetUserName(ClaimsPrincipal principal)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            return principal.FindFirst(_userManager.Options.ClaimsIdentity.UserNameClaimType) is Claim claim ? claim.Value : null;
        }

        public long? GetUserId(ClaimsPrincipal principal)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            return long.TryParse(principal.FindFirst(_userManager.Options.ClaimsIdentity.UserIdClaimType) is Claim claim ? claim.Value : null, out long userId) ? userId : null;
        }

        public Task<User?> GetUser(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
        {
            return _userManager.GetUserAsync(principal);
        }

        public string? GetDeviceId(ClaimsPrincipal principal)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            return principal.FindFirst(ClaimTypes.System) is Claim claim ? claim.Value : null;
        }

        public string? GetSecurityStamp(ClaimsPrincipal principal)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            return principal.FindFirst(ClaimTypes.SerialNumber) is Claim claim ? claim.Value : null;
        }

        public async Task GenerateUserNameAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            user.UserName = await AlgorithmHelper.GenerateSlugAsync($"{user.FirstName} {user.LastName}".ToLower(), userName => _userManager.Users.AnyAsync(_ => _.UserName == userName, cancellationToken));
        }

        [return: NotNullIfNotNull(nameof(phoneNumber))]
        private string? NormalizePhoneNumber(string? phoneNumber)
        {
            if (ValidationHelper.TryParsePhoneNumber(phoneNumber, out var parsedPhoneNumber))
                return ValidationHelper.FormatPhoneNumber(parsedPhoneNumber);

            else return null;
        }
    }
}