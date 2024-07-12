using FluentValidation;

namespace NextSolution.Server.Models.Identity
{
    public class RefreshTokenForm
    {
        public string RefreshToken { get; set; } = null!;
    }

    public class RefreshTokenFormValidator : AbstractValidator<RefreshTokenForm>
    {
        public RefreshTokenFormValidator()
        {
            RuleFor(_ => _.RefreshToken).NotEmpty();
        }
    }
}
