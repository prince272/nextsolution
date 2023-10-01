using Microsoft.AspNetCore.Identity;

namespace NextSolution.Infrastructure.Data
{
    public static class IdentityExtensions
    {
        public static string GetMessage(this IEnumerable<IdentityError> errors)
        {
            return "Operation failed: " + string.Join(string.Empty, errors.Select(x => $"{Environment.NewLine} -- {x.Code}: {x.Description}"));
        }
    }
}
