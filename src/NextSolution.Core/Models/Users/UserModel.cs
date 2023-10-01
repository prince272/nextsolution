using NextSolution.Core.Entities;
using AbstractProfile = AutoMapper.Profile;

namespace NextSolution.Core.Models.Users
{
    public class UserModel
    {
        public long Id { get; set; }

        public string? UserName { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }

        public bool EmailRequired { get; set; }

        public string? PhoneNumber { get; set; }

        public bool PhoneNumberRequired { get; set; }

        public long? AvatarId { get; set; }

        public string? AvatarUrl { get; set; }

        public string? Bio { get; set; }

        public bool Active { get; set; }

        public bool Online { get; set; }

        public DateTimeOffset LastActiveAt { get; set; }

        public IList<string> Roles { get; set; } = new List<string>();
    }

    public class UserModelProfile : AbstractProfile
    {
        public UserModelProfile()
        {
            CreateMap<User, UserModel>();
        }
    }
}
