using FluentValidation;
using Next_Solution.WebApi.Providers.Validation;
using Next_Solution.WebApi.Providers.ModelValidator;

namespace Next_Solution.WebApi.Models.Identity
{
    public class SendConfirmAccountCodeForm
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
    }

    public class ConfirmAccountForm 
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

        public string Code { get; set; } = null!;
    }

    public class SendConfirmAccountCodeFormValidator : AbstractValidator<SendConfirmAccountCodeForm>
    {
        public SendConfirmAccountCodeFormValidator()
        {
            RuleFor(_ => _.Username).NotEmpty().WithName("Email or phone number").DependentRules(() =>
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

    public class ConfirmAccountFormValidator : AbstractValidator<ConfirmAccountForm>
    {
        public ConfirmAccountFormValidator()
        {
            RuleFor(_ => _.Username).NotEmpty().WithName("Email or phone number").DependentRules(() =>
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
