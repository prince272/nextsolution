using NextSolution.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Infrastructure.Identity
{
    public class UserSessionOptions
    {
        public TimeSpan AccessTokenExpiresIn { set; get; }

        public TimeSpan RefreshTokenExpiresIn { set; get; }

        public bool AllowMultipleSessions { set; get; }
    }
}
