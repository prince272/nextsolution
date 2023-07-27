using NextSolution.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Models.Accounts
{
    public class CreateAccountForm
    {
        public string FirstName { get; set; } = default!;

        public string LastName { get; set; } = default!;

        public string Username { get; set; } = default!;

        public string Password { get; set; } = default!;

        public class Validator : AbstractValidator<CreateAccountForm>
        {
            public Validator()
            {
                RuleFor(_ => _.FirstName).NotEmpty();
                RuleFor(_ => _.LastName).NotEmpty();
                RuleFor(_ => _.Username).NotEmpty().Username();
                RuleFor(_ => _.Password).NotEmpty().Password();
            }
        }
    }
}
