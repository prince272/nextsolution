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
    public class UserWithSessionModel : UserModel
    {
        public bool EmailConfirmed { get; set; }

        public bool PhoneNumberConfirmed { get; set; }

        public string? AccessToken { get; set; }

        public string? RefreshToken { get; set; }

        public string? TokenType { get; set; }
    }

    public class UserWithSessionModelProfile : AbstractProfile
    {
        public UserWithSessionModelProfile()
        {
            CreateMap<User, UserWithSessionModel>();
            CreateMap<UserSessionInfo, UserWithSessionModel>();
        }
    }
}

