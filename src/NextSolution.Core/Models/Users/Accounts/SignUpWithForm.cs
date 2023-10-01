using FluentValidation;
using NextSolution.Core.Utilities;
using System.Text.Json.Serialization;

namespace NextSolution.Core.Models.Users.Accounts
{
    public class SignUpWithForm
    {
        public string FirstName { get; set; } = default!;

        public string LastName { get; set; } = default!;

        public string Username { get; set; } = default!;


        [JsonIgnore]
        public ContactType UsernameType => ValidationHelper.GetContactType(Username);

        public string ProviderName { get; set; } = default!;

        public string ProviderKey { get; set; } = default!;

        public string? ProviderDisplayName { get; set; }
    }

    public class SignUpWithFormValidator : AbstractValidator<SignUpWithForm>
    {
        public SignUpWithFormValidator()
        {
            RuleFor(_ => _.FirstName);
            RuleFor(_ => _.LastName);
            RuleFor(_ => _.Username).NotEmpty().Username();
            RuleFor(_ => _.ProviderName).NotEmpty();
            RuleFor(_ => _.ProviderKey).NotEmpty();
        }
    }
}
