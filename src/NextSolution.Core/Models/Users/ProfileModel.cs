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
    public class ProfileModel
    {
        public long Id { get; set; }

        public string? UserName { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public bool Active { get; set; }

        public bool Online { get; set; }

        public DateTimeOffset ActiveAt { get; set; }
    }

    public class ProfileProfile : AbstractProfile
    {
        public ProfileProfile()
        {
            CreateMap<User, ProfileModel>();
        }
    }
}
