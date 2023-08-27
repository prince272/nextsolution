using NextSolution.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Models.Accounts
{
    public class SignOutForm
    {
        public string RefreshToken { get; set; } = default!;

        public class Validator : AbstractValidator<SignOutForm>
        {
            public Validator()
            {
                RuleFor(_ => _.RefreshToken).NotEmpty();
            }
        }
    }
}
