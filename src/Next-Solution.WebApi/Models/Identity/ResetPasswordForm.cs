using FluentValidation;
using Next_Solution.WebApi.Providers.Validation;
using Next_Solution.WebApi.Providers.ModelValidator;

namespace Next_Solution.WebApi.Models.Identity
{
    public class ResetPasswordSendCodeForm
    {
        public string Username { get; set; } = null!;

        private ContactType? usernameType;
        public ContactType? UsernameType
        {
            get
            {
                usernameType ??= (!string.IsNullOrWhiteSpace(Username) ? ValidationHelper.DetermineContactType(Username) : null);
                return usernameType;
            }
            set => usernameType = value;
        }

        public string NewPassword { get; set; } = null!;
    }

    public class ResetPasswordVerifyCodeForm : ResetPasswordSendCodeForm
    {
        public string Code { get; set; } = null!;
    }

    public class ResetPasswordSendCodeFormValidator : AbstractValidator<ResetPasswordSendCodeForm>
    {
        public ResetPasswordSendCodeFormValidator()
        {
            RuleFor(_ => _.Username).NotEmpty().DependentRules(() =>
            {
                When(_ => _.UsernameType!.Value == ContactType.Email, () =>
                {
                    RuleFor(_ => _.Username).Email().WithName("Email");
                });

                When(_ => _.UsernameType!.Value == ContactType.PhoneNumber, () =>
                {
                    RuleFor(_ => _.Username).PhoneNumber().WithName("Phone number");
                });
            });
        }
    }

    public class ResetPasswordVerifyCodeFormValidator : AbstractValidator<ResetPasswordVerifyCodeForm>
    {
        public ResetPasswordVerifyCodeFormValidator()
        {
            RuleFor(_ => _.Username).NotEmpty().DependentRules(() =>
            {
                When(_ => _.UsernameType!.Value == ContactType.Email, () =>
                {
                    RuleFor(_ => _.Username).Email().WithName("Email");
                });

                When(_ => _.UsernameType!.Value == ContactType.PhoneNumber, () =>
                {
                    RuleFor(_ => _.Username).PhoneNumber().WithName("Phone number");
                });
            });

            RuleFor(_ => _.Code).NotEmpty();
        }
    }
}
