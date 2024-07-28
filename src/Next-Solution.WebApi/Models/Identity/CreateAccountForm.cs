using AutoMapper;
using FluentValidation;
using Next_Solution.WebApi.Data.Entities.Identity;
using Next_Solution.WebApi.Providers.ModelValidator;
using Next_Solution.WebApi.Providers.Validation;

namespace Next_Solution.WebApi.Models.Identity
{
    public class CreateAccountForm
    {
        public string FirstName { get; set; } = null!;

        public string? LastName { get; set; }

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

    public class CreateAccountFormValidator : AbstractValidator<CreateAccountForm>
    {
        public CreateAccountFormValidator()
        {
            RuleFor(_ => _.FirstName).NotEmpty().MaximumLength(256);
            RuleFor(_ => _.LastName).MaximumLength(256);
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
            RuleFor(_ => _.Password).NotEmpty().Password();
        }
    }

    public class CreateAccountFormProfile : Profile
    {
        public CreateAccountFormProfile()
        {
            CreateMap<CreateAccountForm, User>()
                .ForMember(_ => _.UserName, _ => _.Ignore())
                .ForMember(_ => _.Email, _ => _.Ignore())
                .ForMember(_ => _.PhoneNumber, _ => _.Ignore());
        }
    }
}
