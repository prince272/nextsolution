using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NextSolution.Core.Entities;
using NextSolution.Core.Extensions.Identity;
using NextSolution.Core.Utilities;
using NextSolution.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Infrastructure.Identity
{
    public class UserSessionStore : IUserSessionStore
    {
        private readonly AppDbContext _dbContext;
        private readonly IOptions<UserSessionOptions> _userSessionOptions;

        public UserSessionStore(AppDbContext dbContext, IOptions<UserSessionOptions> userSessionOptions)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException();
            _userSessionOptions = userSessionOptions ?? throw new ArgumentNullException(nameof(userSessionOptions));
        }

        public async Task AddSessionAsync(User user, UserSessionInfo session)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (session == null) throw new ArgumentNullException(nameof(session));

            if (!_userSessionOptions.Value.AllowMultipleSessions)
            {
                await _dbContext.Set<UserSession>().Where(_ => _.UserId == user.Id).ForEachAsync(session => _dbContext.Remove(session));
            }

            await _dbContext.AddAsync(new UserSession
            {
                UserId = user.Id,

                AccessTokenHash = AlgorithmHelper.GenerateHash(session.AccessToken),
                RefreshTokenHash = AlgorithmHelper.GenerateHash(session.RefreshToken),

                AccessTokenExpiresAt = session.RefreshTokenExpiresAt,
                RefreshTokenExpiresAt = session.RefreshTokenExpiresAt
            });
            await _dbContext.SaveChangesAsync();
        }

        public async Task RemoveSessionAsync(User user, string token)
        {
            if (!_userSessionOptions.Value.AllowMultipleSessions)
            {
                await _dbContext.Set<UserSession>().Where(_ => _.UserId == user.Id).ForEachAsync(session => _dbContext.Remove(session));
            }
            else
            {
                var tokenHash = AlgorithmHelper.GenerateHash(token);
                var current = DateTimeOffset.UtcNow;

                await _dbContext.Set<UserSession>()
                    .Where(_ => _.UserId == user.Id)
                    .Where(_ => _.AccessTokenHash == tokenHash || _.RefreshTokenHash == tokenHash)
                    .Where(_ => _.AccessTokenExpiresAt >= current || _.RefreshTokenExpiresAt >= current)
                    .ForEachAsync(session => _dbContext.Remove(session));
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<User?> FindUserByAccessTokenAsync(string accessToken)
        {
            if (accessToken == null) throw new ArgumentNullException(nameof(accessToken));

            var accessTokenHash = AlgorithmHelper.GenerateHash(accessToken);

            var session = await _dbContext.Set<UserSession>().FirstOrDefaultAsync(_ => _.RefreshTokenHash == accessTokenHash);

            if (session == null || session.AccessTokenExpiresAt >= DateTimeOffset.UtcNow) return null;
            return await _dbContext.FindAsync<User>(session.UserId);
        }

        public async Task<User?> FindUserByRefreshTokenAsync(string refreshToken)
        {
            if (refreshToken == null) throw new ArgumentNullException(nameof(refreshToken));

            var refreshTokenHash = AlgorithmHelper.GenerateHash(refreshToken);

            var session = await _dbContext.Set<UserSession>().FirstOrDefaultAsync(_ => _.RefreshTokenHash == refreshTokenHash);

            if (session == null || session.RefreshTokenExpiresAt >= DateTimeOffset.UtcNow) return null;
            return await _dbContext.FindAsync<User>(session.UserId);
        }
    }
}