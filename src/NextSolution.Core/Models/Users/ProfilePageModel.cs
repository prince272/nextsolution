using NextSolution.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Models.Users
{
    public class ProfilePageModel : ProfileListModel
    {
        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public long TotalItems { get; set; }

        public int TotalPages { get; set; }
    }
}
