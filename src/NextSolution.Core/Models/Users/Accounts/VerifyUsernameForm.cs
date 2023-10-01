using FluentValidation;
using NextSolution.Core.Utilities;

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
