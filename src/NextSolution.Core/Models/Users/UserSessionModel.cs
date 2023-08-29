using AbstractProfile = AutoMapper.Profile;
using NextSolution.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextSolution.Core.Extensions.Identity;

namespace NextSolution.Core.Models.Users
{
    public class UserSessionModel : ProfileModel
    {
        public bool EmailConfirmed { get; set; }

        public bool PhoneNumberConfirmed { get; set; }

        public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();

        public string? AccessToken { get; set; }

        public string? RefreshToken { get; set; }

        public string? TokenType { get; set; }
    }

    public class UserSessionProfile : AbstractProfile
    {
        public UserSessionProfile()
        {
            CreateMap<User, UserSessionModel>();
            CreateMap<UserSessionInfo, UserSessionModel>();
        }
    }
}

