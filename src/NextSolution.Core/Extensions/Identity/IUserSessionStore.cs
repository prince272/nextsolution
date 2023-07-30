using NextSolution.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Extensions.Identity
{
    public interface IUserSessionStore
    {
        Task AddSessionAsync(User user, UserSessionInfo session);

        Task RemoveSessionAsync(User user, string token);

        Task<User?> FindUserByAccessTokenAsync(string accessToken);

        Task<User?> FindUserByRefreshTokenAsync(string refreshToken);
    }
}
