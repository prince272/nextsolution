using FluentValidation;
using Next_Solution.WebApi.Providers.ModelValidator;
using Next_Solution.WebApi.Providers.Validation;
using System.Text.Json.Serialization;

namespace Next_Solution.WebApi.Models.Identity
{
    public class SendConfirmAccountCodeForm
    {
        public string Username { get; set; } = null!;

        [JsonIgnore]
        public ContactType UsernameType
        {
            get => !string.IsNullOrWhiteSpace(Username) ? ValidationHelper.DetermineContactType(Username) : default;
        }
    }

    public class ConfirmAccountForm
    {
        public string Username { get; set; } = null!;

        [JsonIgnore]
        public ContactType UsernameType
        {
            get => !string.IsNullOrWhiteSpace(Username) ? ValidationHelper.DetermineContactType(Username) : default;
        }

        public string Code { get; set; } = null!;
    }

    public class SendConfirmAccountCodeFormValidator : AbstractValidator<SendConfirmAccountCodeForm>
    {
        public SendConfirmAccountCodeFormValidator()
        {
            RuleFor(_ => _.Username).NotEmpty().WithName("Email or phone number").DependentRules(() =>
            {
                When(_ => _.UsernameType == ContactType.Email, () =>
                {
                    RuleFor(_ => _.Username).Email().WithName("Email");
                });

                When(_ => _.UsernameType == ContactType.PhoneNumber, () =>
                {
                    RuleFor(_ => _.Username).PhoneNumber().WithName("Phone number");
                });
            });
        }
    }

    public class ConfirmAccountFormValidator : AbstractValidator<ConfirmAccountForm>
    {
        public ConfirmAccountFormValidator()
        {
            RuleFor(_ => _.Username).NotEmpty().WithName("Email or phone number").DependentRules(() =>
            {
                When(_ => _.UsernameType == ContactType.Email, () =>
                {
                    RuleFor(_ => _.Username).Email().WithName("Email");
                });

                When(_ => _.UsernameType == ContactType.PhoneNumber, () =>
                {
                    RuleFor(_ => _.Username).PhoneNumber().WithName("Phone number");
                });
            });

            RuleFor(_ => _.Code).NotEmpty();
        }
    }
}
