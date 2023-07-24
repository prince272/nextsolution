using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Models.Accounts
{
    public class SignInAccountForm
    {
        public string Username { get; set; } = default!;

        public string Password { get; set; } = default!;

        public class Validator : AbstractValidator<CreateAccountForm>
        {
            public Validator()
            {
                RuleFor(_ => _.Username).NotEmpty();
                RuleFor(_ => _.Password).NotEmpty();
            }
        }
    }
}
