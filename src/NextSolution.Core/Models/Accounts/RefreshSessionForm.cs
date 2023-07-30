using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Models.Accounts
{
    public class RefreshSessionForm
    {
        public string RefreshToken { get; set; } = default!;

        public class Validator : AbstractValidator<RefreshSessionForm>
        {
            public Validator()
            {
                RuleFor(_ => _.RefreshToken).NotEmpty();
            }
        }
    }
}
