using FluentValidation;
using NextSolution.Core.Utilities;
using System.Text.Json.Serialization;

namespace NextSolution.Core.Models.Users.Accounts
{
    public class SignInForm
    {
        public string Username { get; set; } = default!;

        [JsonIgnore]
        public ContactType UsernameType => ValidationHelper.GetContactType(Username);

        public string Password { get; set; } = default!;
    }

    public class SignInFormValidator : AbstractValidator<SignInForm>
    {
        public SignInFormValidator()
        {
            RuleFor(_ => _.Username).NotEmpty().Username();
            RuleFor(_ => _.Password).NotEmpty();
        }
    }
}
