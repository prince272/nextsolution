using FluentValidation;
using Next_Solution.WebApi.Providers.Validation;
using Next_Solution.WebApi.Providers.ModelValidator;
using System.Text.Json.Serialization;

namespace Next_Solution.WebApi.Models.Identity
{
    public class SendChangeAccountCodeForm
    {
        [JsonIgnore]
        public ContactType NewUsernameType
        {
            get => !string.IsNullOrWhiteSpace(NewUsername) ? ValidationHelper.DetermineContactType(NewUsername) : default;
        }

        public string NewUsername { get; set; } = null!;
    }

    public class ChangeAccountForm
    {
        [JsonIgnore]
        public ContactType NewUsernameType
        {
            get => !string.IsNullOrWhiteSpace(NewUsername) ? ValidationHelper.DetermineContactType(NewUsername) : default;
        }

        public string NewUsername { get; set; } = null!;

        public string Code { get; set; } = null!;
    }

    public class SendChangeAccountCodeFormValidator : AbstractValidator<SendChangeAccountCodeForm>
    {
        public SendChangeAccountCodeFormValidator()
        {
            RuleFor(_ => _.NewUsername).NotEmpty().WithName("New email or phone number").DependentRules(() =>
            {
                When(_ => _.NewUsernameType == ContactType.Email, () =>
                {
                    RuleFor(_ => _.NewUsername).Email().WithName("New email");
                });

                When(_ => _.NewUsernameType == ContactType.PhoneNumber, () =>
                {
                    RuleFor(_ => _.NewUsername).PhoneNumber().WithName("New phone number");
                });
            });
        }
    }

    public class ChangeAccountFormValidator : AbstractValidator<ChangeAccountForm>
    {
        public ChangeAccountFormValidator()
        {
            RuleFor(_ => _.NewUsername).NotEmpty().WithName("New email or phone number").DependentRules(() =>
            {
                When(_ => _.NewUsernameType == ContactType.Email, () =>
                {
                    RuleFor(_ => _.NewUsername).Email().WithName("New email");
                });

                When(_ => _.NewUsernameType == ContactType.PhoneNumber, () =>
                {
                    RuleFor(_ => _.NewUsername).PhoneNumber().WithName("New phone number");
                });
            });

            RuleFor(_ => _.Code).NotEmpty();
        }
    }
}
