using AbstractProfile = AutoMapper.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextSolution.Core.Entities;
using NextSolution.Core.Extensions.Identity;

namespace NextSolution.Core.Models.Users
{
    public class UserProfileModel
    {
        public long Id { get; set; }

        public string? UserName { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }
    }

    public class UserProfileProfile : AbstractProfile
    {
        public UserProfileProfile()
        {
            CreateMap<User, UserProfileModel>();
        }
    }
}
