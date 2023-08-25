using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Infrastructure.Data.Extensions
{
    public static class IdentityExtensions
    {
        public static string GetMessage(this IEnumerable<IdentityError> errors)
        {
            return "Operation failed: " + string.Join(string.Empty, errors.Select(x => $"{Environment.NewLine} -- {x.Code}: {x.Description}"));
        }
    }
}
