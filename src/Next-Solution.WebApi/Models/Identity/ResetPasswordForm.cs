using FluentValidation;
using Next_Solution.WebApi.Providers.ModelValidator;
using Next_Solution.WebApi.Providers.Validation;
using System.Text.Json.Serialization;

namespace Next_Solution.WebApi.Models.Identity
{
    public class SendResetPasswordCodeForm
    {
        public string Username { get; set; } = null!;

        [JsonIgnore]
        public ContactType UsernameType
        {
            get => !string.IsNullOrWhiteSpace(Username) ? ValidationHelper.DetermineContactType(Username) : default;
        }
    }

    public class ResetPasswordForm : SendResetPasswordCodeForm
    {
        public string NewPassword { get; set; } = null!;

        public string ConfirmPassword { get; set; } = null!;

        public string Code { get; set; } = null!;
    }

    public class SendResetPasswordCodeFormValidator : AbstractValidator<SendResetPasswordCodeForm>
    {
        public SendResetPasswordCodeFormValidator()
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

    public class ResetPasswordFormValidator : AbstractValidator<ResetPasswordForm>
    {
        public ResetPasswordFormValidator()
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

            RuleFor(_ => _.NewPassword).NotEmpty().Password();

            RuleFor(_ => _.ConfirmPassword).NotEmpty().Equal(_ => _.NewPassword);
        }
    }
}
