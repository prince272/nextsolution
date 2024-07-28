using FluentValidation;
using Next_Solution.WebApi.Providers.ModelValidator;

namespace Next_Solution.WebApi.Models.Identity
{
    public class ChangePasswordForm
    {
        public string? OldPassword { get; set; }

        public string NewPassword { get; set; } = null!;

        public string ConfirmPassword { get; set; } = null!;
    }

    public class ChangePasswordFormValidator : AbstractValidator<ChangePasswordForm>
    {
        public ChangePasswordFormValidator()
        {
            RuleFor(_ => _.NewPassword).NotEmpty().MaximumLength(128).Password();

            RuleFor(_ => _.ConfirmPassword).NotEmpty().MaximumLength(128).Equal(_ => _.NewPassword)
                .WithMessage("'Confirm password' must be equal to 'New password'");
        }
    }
}
