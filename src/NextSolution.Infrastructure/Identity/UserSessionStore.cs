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

        public async Task AddSessionAsync(User user, UserSessionInfo session, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (session == null) throw new ArgumentNullException(nameof(session));

            if (!_userSessionOptions.Value.AllowMultipleSessions)
            {
                await _dbContext.Set<UserSession>().Where(_ => _.UserId == user.Id).ForEachAsync(session => _dbContext.Remove(session), cancellationToken);
            }

            await _dbContext.AddAsync(new UserSession
            {
                UserId = user.Id,

                AccessTokenHash = AlgorithmHelper.GenerateSHA256Hash(session.AccessToken),
                RefreshTokenHash = AlgorithmHelper.GenerateSHA256Hash(session.RefreshToken),

                AccessTokenExpiresAt = session.RefreshTokenExpiresAt,
                RefreshTokenExpiresAt = session.RefreshTokenExpiresAt
            }, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveSessionAsync(User user, string token, CancellationToken cancellationToken = default)
        {
            if (!_userSessionOptions.Value.AllowMultipleSessions)
            {
                await _dbContext.Set<UserSession>().Where(_ => _.UserId == user.Id).ForEachAsync(session => _dbContext.Remove(session), cancellationToken);
            }
            else
            {
                var tokenHash = AlgorithmHelper.GenerateSHA256Hash(token);
                var currentTime = DateTimeOffset.UtcNow;

                await _dbContext.Set<UserSession>()
                    .Where(_ => _.UserId == user.Id)
                    .Where(_ => _.AccessTokenHash == tokenHash || _.RefreshTokenHash == tokenHash)
                    .ForEachAsync(session => _dbContext.Remove(session), cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<User?> FindUserByAccessTokenAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            if (accessToken == null) throw new ArgumentNullException(nameof(accessToken));

            var accessTokenHash = AlgorithmHelper.GenerateSHA256Hash(accessToken);

            var session = await _dbContext.Set<UserSession>().FirstOrDefaultAsync(_ => _.RefreshTokenHash == accessTokenHash, cancellationToken);

            if (session == null || session.AccessTokenExpiresAt < DateTimeOffset.UtcNow) return null;
            return await _dbContext.FindAsync<User>(keyValues: new object[] { session.UserId }, cancellationToken);
        }

        public async Task<User?> FindUserByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            if (refreshToken == null) throw new ArgumentNullException(nameof(refreshToken));

            var refreshTokenHash = AlgorithmHelper.GenerateSHA256Hash(refreshToken);

            var session = await _dbContext.Set<UserSession>().FirstOrDefaultAsync(_ => _.RefreshTokenHash == refreshTokenHash, cancellationToken);

            if (session == null || session.RefreshTokenExpiresAt < DateTimeOffset.UtcNow) return null;
            return await _dbContext.FindAsync<User>(keyValues: new object[] { session.UserId }, cancellationToken);
        }
    }
}