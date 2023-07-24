using FluentValidation.Results;
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NextSolution.Core.Helpers
{
    public static class ValidationHelper
    {
        public static MailAddress ParseEmail(string value)
        {
            ArgumentException.ThrowIfNullOrEmpty(value?.Trim(), nameof(value));

            var emailAddress = new MailAddress(value);

            if (emailAddress.Address == value)
            {
                return emailAddress;
            }

            throw new FormatException($"Input '{value}' was not recognized as a valid email address.");
        }

        public static PhoneNumber ParsePhoneNumber(string value)
        {
            ArgumentException.ThrowIfNullOrEmpty(value?.Trim(), nameof(value));

            try
            {
                var phoneNumberHelper = PhoneNumberUtil.GetInstance();
                var phoneNumber = phoneNumberHelper.ParseAndKeepRawInput(value, null);

                if (phoneNumberHelper.IsValidNumber(phoneNumber) && phoneNumber.RawInput == value)
                {
                    return phoneNumber;
                }
            }
            catch (NumberParseException) { }

            throw new FormatException($"Input '{value}' was not recognized as a valid phone number.");
        }

        public static bool TryParseEmail(string value, [NotNullWhen(true)] out MailAddress? email)
        {
            try
            {
                email = ParseEmail(value);
                return true;
            }
            catch (Exception)
            {
                email = null;
                return false;
            }
        }

        public static bool TryParsePhoneNumber(string value, [NotNullWhen(true)] out PhoneNumber? phoneNumber)
        {
            try
            {
                phoneNumber = ParsePhoneNumber(value);
                return true;
            }
            catch (Exception)
            {
                phoneNumber = null;
                return false;
            }
        }

        public static TextFormat CheckFormat(string value)
        {
            ArgumentException.ThrowIfNullOrEmpty(value?.Trim(), nameof(value));

            if (!Regex.IsMatch(value.ToLowerInvariant(), "^[-+0-9() ]+$"))
            {
                return TextFormat.EmailAddress;
            }
            else
            {
                return TextFormat.PhoneNumber;
            }
        }
    }

    public enum TextFormat
    {
        EmailAddress,
        PhoneNumber
    }

    public static class ValidationExtensions
    {
        public static IRuleBuilder<T, string> Username<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.Custom((value, context) =>
            {
                var valueFormat = ValidationHelper.CheckFormat(value);

                if (valueFormat == TextFormat.EmailAddress)
                {
                    if (!ValidationHelper.TryParseEmail(value, out var _))
                        context.AddFailure($"'Email address' is not valid.");
                }
                else if (valueFormat == TextFormat.PhoneNumber)
                {
                    if (!ValidationHelper.TryParsePhoneNumber(value, out var _))
                        context.AddFailure($"'Phone number' is not valid.");
                }
                else
                {
                    throw new InvalidOperationException($"Input '{value}' was not recognized as a valid email or phone number.");
                }
            });

            return ruleBuilder;
        }

        // How can I create strong passwords with FluentValidation?
        // source: https://stackoverflow.com/questions/63864594/how-can-i-create-strong-passwords-with-fluentvalidation
        public static IRuleBuilder<T, string> Password<T>(this IRuleBuilder<T, string> ruleBuilder, int minimumLength = 6)
        {
            var options = ruleBuilder
                .MinimumLength(minimumLength)
                .Matches("[A-Z]").WithMessage("'{PropertyName}' must contain at least 1 upper case.")
                .Matches("[a-z]").WithMessage("'{PropertyName}' must contain at least 1 lower case.")
                .Matches("[0-9]").WithMessage("'{PropertyName}' must contain at least 1 digit.")
                .Matches("[^a-zA-Z0-9]").WithMessage("'{PropertyName}' must contain at least 1 special character.");

            return options;
        }

        public static IDictionary<string, string[]> ToDictionary(this IEnumerable<ValidationFailure> errors)
        {
            return errors
              .GroupBy(x => x.PropertyName)
              .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ErrorMessage).ToArray()
              );
        }
    }
}