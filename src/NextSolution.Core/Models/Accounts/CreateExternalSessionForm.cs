using NextSolution.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Models.Accounts
{
    public class CreateExternalSessionForm
    {
        public ClaimsPrincipal Principal { get; set; } = default!;

        public string ProviderName { get; set; } = default!;

        public string ProviderKey { get; set; } = default!;

        public string? ProviderDisplayName { get; set; }

        public class Validator : AbstractValidator<CreateExternalSessionForm>
        {
            public Validator()
            {
                RuleFor(_ => _.Principal).NotEmpty();
                RuleFor(_ => _.ProviderName).NotEmpty();
                RuleFor(_ => _.ProviderKey).NotEmpty();
            }
        }
    }
}
