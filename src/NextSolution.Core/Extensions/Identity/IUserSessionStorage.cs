using NextSolution.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Extensions.Identity
{
    public interface IUserSessionStorage
    {
        Task AddSessionAsync(User user, UserSessionInfo session, CancellationToken cancellationToken = default);

        Task RemoveSessionAsync(User user, string token, CancellationToken cancellationToken = default);

        Task<User?> GetUserByAccessTokenAsync(string accessToken, CancellationToken cancellationToken = default);

        Task<User?> GetUserByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    }
}
