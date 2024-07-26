using NextSolution._1.Server.Data.Entities.Identity;
using NextSolution._1.Server.Providers.Validation;
using AutoMapper;
using FluentValidation;

namespace NextSolution._1.Server.Models.Identity
{
    public class SignInForm
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

        public string Password { get; set; } = null!;
    }

    public class SignInFormValidator : AbstractValidator<SignInForm>
    {
        public SignInFormValidator()
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
            RuleFor(_ => _.Password).NotEmpty().MaximumLength(256);
        }
    }

    public class SignInFormProfile : Profile
    {
        public SignInFormProfile()
        {
            CreateMap<SignInForm, User>()
                .ForMember(_ => _.UserName, _ => _.Ignore())
                .ForMember(_ => _.Email, _ => _.Ignore())
                .ForMember(_ => _.PhoneNumber, _ => _.Ignore());
        }
    }

    public enum SignInProvider
    {
        Google,
        Facebook
    }
}
