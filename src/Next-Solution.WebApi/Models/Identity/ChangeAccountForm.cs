using FluentValidation;
using Next_Solution.WebApi.Providers.Validation;
using Next_Solution.WebApi.Providers.ModelValidator;

namespace Next_Solution.WebApi.Models.Identity
{
    public class ChangeAccountSendCodeForm
    {
        private ContactType? newUsernameType;
        public ContactType? NewUsernameType
        {
            get
            {
                newUsernameType ??= (!string.IsNullOrWhiteSpace(NewUsername) ? ValidationHelper.DetermineContactType(NewUsername) : null);
                return newUsernameType;
            }
            set => newUsernameType = value;
        }

        public string NewUsername { get; set; } = null!;
    }

    public class ChangeAccountVerifyCodeForm
    {
        private ContactType? newUsernameType;
        public ContactType? NewUsernameType
        {
            get
            {
                newUsernameType ??= (!string.IsNullOrWhiteSpace(NewUsername) ? ValidationHelper.DetermineContactType(NewUsername) : null);
                return newUsernameType;
            }
            set => newUsernameType = value;
        }

        public string NewUsername { get; set; } = null!;

        public string Code { get; set; } = null!;
    }

    public class ChangeAccountSendCodeFormValidator : AbstractValidator<ChangeAccountSendCodeForm>
    {
        public ChangeAccountSendCodeFormValidator()
        {
            RuleFor(_ => _.NewUsername).NotEmpty().DependentRules(() =>
            {
                When(_ => _.NewUsernameType!.Value == ContactType.Email, () =>
                {
                    RuleFor(_ => _.NewUsername).Email().WithName("New email");
                });

                When(_ => _.NewUsernameType!.Value == ContactType.PhoneNumber, () =>
                {
                    RuleFor(_ => _.NewUsername).PhoneNumber().WithName("New phone number");
                });
            });
        }
    }

    public class ChangeAccountVerifyCodeFormValidator : AbstractValidator<ChangeAccountVerifyCodeForm>
    {
        public ChangeAccountVerifyCodeFormValidator()
        {
            RuleFor(_ => _.NewUsername).NotEmpty().DependentRules(() =>
            {
                When(_ => _.NewUsernameType!.Value == ContactType.Email, () =>
                {
                    RuleFor(_ => _.NewUsername).Email().WithName("New email");
                });

                When(_ => _.NewUsernameType!.Value == ContactType.PhoneNumber, () =>
                {
                    RuleFor(_ => _.NewUsername).PhoneNumber().WithName("New phone number");
                });
            });

            RuleFor(_ => _.Code).NotEmpty();
        }
    }
}
