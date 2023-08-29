using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Models.Users
{
    public class UserListModel
    {
        public IEnumerable<UserModel> Items { get; set; } = new List<UserModel>();
    }

}
