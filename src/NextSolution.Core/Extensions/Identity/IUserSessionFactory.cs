using NextSolution.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Extensions.Identity
{
    public interface IUserSessionFactory
    {
        Task<UserSessionInfo> GenerateAsync(User user, CancellationToken cancellationToken = default);

        Task<bool> ValidateAccessTokenAsync(string accessToken, CancellationToken cancellationToken = default);

        Task<bool> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    }
}
