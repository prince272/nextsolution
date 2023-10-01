using FluentValidation;

namespace NextSolution.Core.Models.Users.Accounts
{
    public class RefreshSessionForm
    {
        public string RefreshToken { get; set; } = default!;
    }

    public class RefreshSessionFormValidator : AbstractValidator<RefreshSessionForm>
    {
        public RefreshSessionFormValidator()
        {
            RuleFor(_ => _.RefreshToken).NotEmpty();
        }
    }
}
