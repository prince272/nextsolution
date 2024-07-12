using NextSolution.Server.Data.Entities.Identity;
using NextSolution.Server.Providers.Validation;
using FluentValidation;
using Humanizer;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace NextSolution.Server.Models.Identity
{
    public class ConfirmAccountForm
    {
        public string Username { get; set; } = null!;

        private ContactType? usernameType;
        public ContactType? UsernameType
        {
            get
            {
                usernameType ??= ValidationHelper.DetermineContactType(Username);
                return usernameType.Value;
            }
            set => usernameType = value;
        }

        public string? Code { get; set; }

        [JsonIgnore]
        [MemberNotNullWhen(false, nameof(Code))]
        public bool SendCode => Process == ConfirmAccountProcess.SendCode;

        public ConfirmAccountProcess Process { get; set; }
    }

    public class ConfirmAccountFormValidator : AbstractValidator<ConfirmAccountForm>
    {
        public ConfirmAccountFormValidator()
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

            RuleFor(_ => _.Code).NotEmpty().When(_ => !_.SendCode);
        }
    }

    public enum ConfirmAccountProcess
    {
        SendCode,
        VerifyCode
    }
}
