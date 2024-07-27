using FluentValidation;
using Next_Solution.WebApi.Providers.Validation;

namespace Next_Solution.WebApi.Providers.Validation
{
    public static class ValidationExtensions
    {
        public static IRuleBuilderOptions<T, string> Email<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.MaximumLength(256).Must((value) =>
            {
                return ValidationHelper.TryParseEmail(value, out var _);
            }).WithMessage("'{PropertyName}' is not valid.");
        }

        public static IRuleBuilderOptions<T, string> PhoneNumber<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.MaximumLength(256).Must((value) =>
            {
                return ValidationHelper.TryParsePhoneNumber(value, out var _);
            }).WithMessage("'{PropertyName}' is not valid.");
        }


        // How can I create strong passwords with FluentValidation?
        // source: https://stackoverflow.com/questions/63864594/how-can-i-create-strong-passwords-with-fluentvalidation
        public static IRuleBuilderOptions<T, string> Password<T>(this IRuleBuilderOptions<T, string> ruleBuilder)
        {
            var options = ruleBuilder
                .MinimumLength(6)
                .MaximumLength(256)
                .Matches("[A-Z]").WithMessage("'{PropertyName}' must contain at least 1 upper case.")
                .Matches("[a-z]").WithMessage("'{PropertyName}' must contain at least 1 lower case.")
                .Matches("[0-9]").WithMessage("'{PropertyName}' must contain at least 1 digit.")
                .Matches("[^a-zA-Z0-9]").WithMessage("'{PropertyName}' must contain at least 1 special character.");

            return options;
        }
    }
}
