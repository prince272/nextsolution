using Microsoft.AspNetCore.Identity;
using NextSolution.Core.Entities;
using NextSolution.Core.Extensions.Identity;
using NextSolution.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task CreateAsync(User user, string password, CancellationToken cancellationToken = default);

        Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

        Task<User?> FindByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);

        Task<bool> IsInRoleAsync(User user, string role, CancellationToken cancellationToken = default);

        Task AddToRoleAsync(User user, string role, CancellationToken cancellationToken = default);

        Task AddToRolesAsync(User user, IEnumerable<string> roles, CancellationToken cancellationToken = default);

        Task RemoveFromRoleAsync(User user, string role, CancellationToken cancellationToken = default);

        Task RemoveFromRolesAsync(User user, IEnumerable<string> roles, CancellationToken cancellationToken = default);

        Task<IEnumerable<string>> GetRolesAsync(User user, CancellationToken cancellationToken = default);

        Task<bool> CheckPasswordAsync(User user, string password, CancellationToken cancellationToken = default);

        Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken = default);

        Task AddPasswordAsync(User user, string password, CancellationToken cancellationToken = default);

        Task ChangePasswordAsync(User user, string currentPassword, string newPassword, CancellationToken cancellationToken = default);

        Task RemovePasswordAsync(User user, CancellationToken cancellationToken = default);

        Task<string> GenerateEmailTokenAsync(User user, CancellationToken cancellationToken = default);

        Task VerifyEmailAsync(User user, string token, CancellationToken cancellationToken = default);

        Task<string> GenerateChangeEmailTokenAsync(User user, string newEmail, CancellationToken cancellationToken = default);

        Task ChangeEmailAsync(User user, string newEmail, string token, CancellationToken cancellationToken = default);


        Task<string> GeneratePhoneNumberTokenAsync(User user, CancellationToken cancellationToken = default);

        Task VerifyPhoneNumberTokenAsync(User user, string token, CancellationToken cancellationToken = default);

        Task<string> GenerateChangePhoneNumberTokenAsync(User user, string newPhoneNumber, CancellationToken cancellationToken = default);

        Task ChangePhoneNumberAsync(User user, string newPhoneNumber, string token, CancellationToken cancellationToken = default);

        Task<string> GeneratePasswordResetTokenAsync(User user, CancellationToken cancellationToken = default);

        Task ResetPasswordAsync(User user, string newPassword, string token, CancellationToken cancellationToken = default);

        Task<UserSessionInfo> GenerateSessionAsync(User user, CancellationToken cancellationToken = default);

        Task AddSessionAsync(User user, UserSessionInfo session, CancellationToken cancellationToken = default);

        Task RemoveSessionAsync(User user, string token, CancellationToken cancellationToken = default);

        Task AddLoginAsync(User user, UserLoginInfo login, CancellationToken cancellationToken = default);

        Task RemoveLoginAsync(User user, string providerName, string providerKey, CancellationToken cancellationToken = default);

        Task<User?> FindByAccessTokenAsync(string accessToken, CancellationToken cancellationToken = default);

        Task<User?> FindByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

        Task<bool> ValidateAccessTokenAsync(string accessToken, CancellationToken cancellationToken = default);

        Task<bool> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

        string? GetDeviceId(ClaimsPrincipal principal);

        long? GetUserId(ClaimsPrincipal principal);

        string? GetUserName(ClaimsPrincipal principal);

        Task<User?> GetUser(ClaimsPrincipal principal, CancellationToken cancellationToken = default);

        string? GetSecurityStamp(ClaimsPrincipal principal);

        Task GenerateUserNameAsync(User user, CancellationToken cancellationToken = default);
    }
}
