using FluentValidation.Results;
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Mail;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NextSolution.Core.Utilities
{
    public static class ValidationHelper
    {
        public static MailAddress ParseEmail(string value)
        {
            ArgumentException.ThrowIfNullOrEmpty(value?.Trim(), nameof(value));

            try
            {
                var emailAddress = new MailAddress(value);

                if (emailAddress.Address == value)
                {
                    return emailAddress;
                }
            }
            catch (Exception) { }

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
            catch (Exception) { }

            throw new FormatException($"Input '{value}' was not recognized as a valid phone number.");
        }

        public static UAParser.ClientInfo ParseUserAgent(string value)
        {
            ArgumentException.ThrowIfNullOrEmpty(value?.Trim(), nameof(value));

            try
            {
                return UAParser.Parser.GetDefault().Parse(value);
            }
            catch (Exception) { }

            throw new FormatException($"Input '{value}' was not recognized as a valid user agent.");
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

        public static string FormatPhoneNumber(PhoneNumber phoneNumber)
        {
            return PhoneNumberUtil.GetInstance().Format(phoneNumber, PhoneNumberFormat.INTERNATIONAL);
        }

        public static ContactType GetContactType(string value)
        {
            ArgumentException.ThrowIfNullOrEmpty(value?.Trim(), nameof(value));

            if (!Regex.IsMatch(value.ToLower(), "^[-+0-9() ]+$"))
            {
                return ContactType.EmailAddress;
            }
            else
            {
                return ContactType.PhoneNumber;
            }
        }
    }

    public enum ContactType
    {
        EmailAddress,
        PhoneNumber
    }
}