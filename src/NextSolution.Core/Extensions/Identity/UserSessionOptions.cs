using NextSolution.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Extensions.Identity
{
    public class UserSessionOptions
    {
        public string Secret { set; get; } = default!;

        public string Issuer { set; get; } = default!;

        public string Audience { set; get; } = default!;

        public TimeSpan AccessTokenExpiresIn { set; get; }

        public TimeSpan RefreshTokenExpiresIn { set; get; }

        public bool AllowMultipleSessions { set; get; }


        public const string ValueSeparator = ";";

        public IEnumerable<string> GetIssuers()
        {
            return Issuer?.Split(ValueSeparator, StringSplitOptions.RemoveEmptyEntries).ToArray() ?? Array.Empty<string>();
        }

        public IEnumerable<string> GetAudiences()
        {
            return Audience?.Split(ValueSeparator, StringSplitOptions.RemoveEmptyEntries).ToArray() ?? Array.Empty<string>();
        }
    }
}
