using PhoneNumbers;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace NextSolution.Core.Utilities
{
    public static class ValidationHelper
    {
        public static MailAddress ParseEmail(string value)
        {
            ArgumentException.ThrowIfNullOrEmpty(value?.Trim(), nameof(value));
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

        public static PhoneNumber ParsePhoneNumber(string value)
        {
            ArgumentException.ThrowIfNullOrEmpty(value?.Trim(), nameof(value));
            Exception? innerException = null;

            try
            {
                var phoneNumberHelper = PhoneNumberUtil.GetInstance();
                var phoneNumber = phoneNumberHelper.ParseAndKeepRawInput(value, null);

                if (phoneNumberHelper.IsValidNumber(phoneNumber) && phoneNumber.RawInput == value)
                {
                    return phoneNumber;
                }

            }
            catch (Exception exception) { innerException = exception; }

            throw new FormatException($"Input '{value}' was not recognized as a valid phone number.", innerException);
        }

        public static UAParser.ClientInfo ParseUserAgent(string value)
        {
            ArgumentException.ThrowIfNullOrEmpty(value?.Trim(), nameof(value));
            Exception? innerException = null;

            try
            {
                return UAParser.Parser.GetDefault().Parse(value);
            }
            catch (Exception exception) { innerException = exception; }

            throw new FormatException($"Input '{value}' was not recognized as a valid user agent.", innerException);
        }

        public static bool TryParseEmail(string? value, [NotNullWhen(true)] out MailAddress? email)
        {
            try
            {
                if (value != null)
                {
                    email = ParseEmail(value);
                    return true;
                }
            }
            catch (Exception) { }

            email = null;
            return false;
        }

        public static bool TryParsePhoneNumber(string? value, [NotNullWhen(true)] out PhoneNumber? phoneNumber)
        {
            try
            {
                if (value != null)
                {
                    phoneNumber = ParsePhoneNumber(value);
                    return true;
                }
            }
            catch (Exception) { }

            phoneNumber = null;
            return false;
        }

        public static bool TryParseUserAgent(string? value, [NotNullWhen(true)] out UAParser.ClientInfo? userAgent)
        {
            try
            {
                if (value != null)
                {
                    userAgent = ParseUserAgent(value);
                    return true;
                }
            }
            catch (Exception) { }

            userAgent = null;
            return false;
        }

        [return: NotNullIfNotNull(nameof(phoneNumber))]
        public static string? NormalizePhoneNumber(string? phoneNumber)
        {
            if (TryParsePhoneNumber(phoneNumber, out var parsedPhoneNumber))
                return PhoneNumberUtil.GetInstance().Format(parsedPhoneNumber, PhoneNumberFormat.E164);

            else return phoneNumber;
        }

        public static ContactType GetContactType(string value)
        {
            ArgumentException.ThrowIfNullOrEmpty(value?.Trim(), nameof(value));

            if (!Regex.IsMatch(value.ToLower(), "^[-+0-9() ]+$"))
            {
                return ContactType.Email;
            }
            else
            {
                return ContactType.PhoneNumber;
            }
        }

        public static bool IsValidPath(string path)
        {
            var fileName = Path.GetFileName(path);

            var invalidFileNameChars = fileName.Where(c => Path.GetInvalidPathChars().Concat(new[] { '/', '\\' }).Contains(c)).ToArray();
            if (invalidFileNameChars.Length > 0) return false;

            var directoryNames = Path.GetDirectoryName(path)?.Split(new char[] { '/', '\\' }) ?? Array.Empty<string>();

            foreach (var directoryName in directoryNames)
            {
                var invalidDirectoryNameChars = directoryName.Where(c => Path.GetInvalidPathChars().Concat(new[] { '/', '\\' }).Contains(c)).ToArray();
                if (invalidDirectoryNameChars.Length > 0) return false;
            }

            return true;
        }
    }

    public enum ContactType
    {
        Email,
        PhoneNumber
    }
}