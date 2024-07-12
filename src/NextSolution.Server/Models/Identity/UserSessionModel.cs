using NextSolution.Server.Data.Entities.Identity;
using NextSolution.Server.Providers.JwtBearer;
using AutoMapper;

namespace NextSolution.Server.Models.Identity
{
    public class UserSessionModel : UserProfileModel
    {
        public string TokenType { get; set; } = null!;
        public string AccessToken { get; set; } = null!;
        public DateTimeOffset AccessTokenExpiresAt { get; set; }
        public string RefreshToken { get; set; } = null!;
        public DateTimeOffset RefreshTokenExpiresAt { get; set; }
    }

    public class UserSessionModelProfile : Profile
    {
        public UserSessionModelProfile()
        {
            CreateMap<User, UserSessionModel>();
            CreateMap<UserProfileModel, UserSessionModel>();
            CreateMap<JwtTokenInfo, UserSessionModel>();
        }
    }
}
