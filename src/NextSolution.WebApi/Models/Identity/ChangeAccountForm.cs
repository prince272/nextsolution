using NextSolution.WebApi.Providers.Validation;
using FluentValidation;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using NextSolution.WebApi.Providers.Validation;

namespace NextSolution.WebApi.Models.Identity
{
    public class ChangeAccountForm
    {
        private ContactType? newUsernameType;
        public ContactType? NewUsernameType
        {
            get
            {
                newUsernameType ??= ValidationHelper.DetermineContactType(NewUsername);
                return newUsernameType.Value;
            }
            set => newUsernameType = value;
        }

        public string NewUsername { get; set; } = null!;

        public string? Code { get; set; }

        [JsonIgnore]
        [MemberNotNullWhen(false, nameof(Code))]
        public bool SendCode => Process == ChangeAccountProcess.SendCode;

        public ChangeAccountProcess Process { get; set; }
    }

    public class ChangeAccountFormValidator : AbstractValidator<ChangeAccountForm>
    {
        public ChangeAccountFormValidator()
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

            RuleFor(_ => _.Code).NotEmpty().When(_ => !_.SendCode);
        }
    }

    public enum ChangeAccountProcess
    {
        SendCode,
        VerifyCode
    }
}
