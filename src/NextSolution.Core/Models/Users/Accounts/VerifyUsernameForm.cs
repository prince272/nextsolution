using FluentValidation;
using NextSolution.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NextSolution.Core.Models.Users.Accounts
{
    public class VerifyUsernameForm
    {
        public string Username { get; set; } = default!;

        public ContactType UsernameType { get; set; }

        public string Code { get; set; } = default!;
    }

    public class VerifyUsernameFormValidator : AbstractValidator<VerifyUsernameForm>
    {
        public VerifyUsernameFormValidator()
        {
            RuleFor(_ => _.Username).NotEmpty().Username();
            RuleFor(_ => _.Code).NotEmpty();
        }
    }
}
