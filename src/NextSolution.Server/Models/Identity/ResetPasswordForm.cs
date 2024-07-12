using NextSolution.Server.Providers.Validation;
using FluentValidation;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace NextSolution.Server.Models.Identity
{
    public class ResetPasswordForm
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
        [MemberNotNullWhen(false, nameof(NewPassword))]
        public bool SendCode => Process == ResetPasswordProcess.SendCode;

        public string? NewPassword { get; set; }

        public ResetPasswordProcess Process { get; set; }
    }

    public class ResetPasswordFormValidator : AbstractValidator<ResetPasswordForm>
    {
        public ResetPasswordFormValidator()
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

            RuleFor(_ => _.NewPassword!).NotEmpty().Password().When(_ => !_.SendCode);
        }
    }

    public enum ResetPasswordProcess
    {
        SendCode,
        VerifyCode
    }
}
