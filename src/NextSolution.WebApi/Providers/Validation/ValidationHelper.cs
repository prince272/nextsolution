using NextSolution.WebApi.Models.Identity;
using PhoneNumbers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace NextSolution.WebApi.Providers.Validation
{
    public static partial class ValidationHelper
    {
        public static MailAddress ParseEmail(string value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));

            Exception? innerException = null;

            try
            {
                var emailAddress = new MailAddress(value);

                if (emailAddress.Address == value)
                {
                    return emailAddress;
                }
            }
            catch (Exception exception) { innerException = exception; }

            throw new FormatException($"Input '{value}' was not recognized as a valid email address.", innerException);
        }

        public static PhoneNumber ParsePhoneNumber(string value, string? regionCode = null)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));

            Exception? innerException = null;

            try
            {
                regionCode ??= RegionInfo.CurrentRegion.TwoLetterISORegionName;
                var phoneNumberHelper = PhoneNumberUtil.GetInstance();
                var phoneNumber = phoneNumberHelper.Parse(value, regionCode);

                if (phoneNumberHelper.IsValidNumber(phoneNumber))
                {
                    return phoneNumber;
                }

            }
            catch (Exception exception) { innerException = exception; }

            throw new FormatException($"Input '{value}' was not recognized as a valid phone number.", innerException);
        }

        public static bool TryParseEmail(string value, [MaybeNullWhen(false)] out MailAddress emailAddress)
        {
            if (value is null)
            {
                emailAddress = default;
                return false;
            }

            try
            {
                emailAddress = ParseEmail(value);
                return true;
            }
            catch (FormatException)
            {
                emailAddress = default;
                return false;
            }
        }

        public static bool TryParsePhoneNumber(string value, [MaybeNullWhen(false)] out PhoneNumber phoneNumber, string? regionCode = null)
        {
            if (value is null)
            {
                phoneNumber = default;
                return false;
            }

            try
            {
                phoneNumber = ParsePhoneNumber(value, regionCode);
                return true;
            }
            catch (FormatException)
            {
                phoneNumber = default;
                return false;
            }
        }

        public static string NormalizeEmail(string value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            try
            {
                var result = ParseEmail(value);
                return result.Address.ToLower();
            }
            catch (FormatException)
            {
                return value;
            }
        }

        public static string NormalizePhoneNumber(string value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            try
            {
                var result = ParsePhoneNumber(value);
                return $"+{result.CountryCode}{result.NationalNumber}";
            }
            catch (FormatException)
            {
                return value;
            }
        }

        public static ContactType DetermineContactType(string value)
        {
            return value is null
                ? throw new ArgumentNullException(nameof(value))
                : !ContactTypeRegex().IsMatch(value.ToLower()) ? ContactType.Email : ContactType.PhoneNumber;
        }

        [GeneratedRegex("^[-+0-9() ]+$")]
        private static partial Regex ContactTypeRegex();
    }
}
