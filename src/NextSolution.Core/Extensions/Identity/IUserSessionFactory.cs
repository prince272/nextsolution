using NextSolution.Core.Entities;

namespace NextSolution.Core.Extensions.Identity
{
    public interface IUserSessionFactory
    {
        Task<UserSessionInfo> GenerateAsync(User user, CancellationToken cancellationToken = default);

        Task<bool> ValidateAccessTokenAsync(string accessToken, CancellationToken cancellationToken = default);

        Task<bool> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    }
}
