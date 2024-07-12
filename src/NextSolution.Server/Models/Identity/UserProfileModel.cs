using NextSolution.Server.Data.Entities.Identity;
using AutoMapper;

namespace NextSolution.Server.Models.Identity
{
    public class UserProfileModel
    {
        public string Id { get; set; } = null!;

        public string FirstName { get; set; } = null!;

        public string? LastName { get; set; }

        public string UserName { get; set; } = null!;

        public string? Email { get; set; }

        public bool EmailConfirmed { get; set; }

        public string? PhoneNumber { get; set; }

        public bool PhoneNumberConfirmed { get; set; }

        public bool PasswordConfigured { get; set; }

        public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();
    }

    public class UserProfileModelProfile : Profile
    {
        public UserProfileModelProfile()
        {
            CreateMap<User, UserProfileModel>();
        }
    }
}
