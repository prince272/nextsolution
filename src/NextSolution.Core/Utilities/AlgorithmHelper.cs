using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace NextSolution.Core.Utilities
{
    public static class AlgorithmHelper
    {
        public static async Task<string> GenerateSlugAsync(string text, Func<string, Task<bool>> exists, string separator = "-")
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            if (separator == null)
                throw new ArgumentNullException(nameof(text));

            string slug = null!;
            int count = 1;

            do
            {
                slug = GenerateSlug($"{text}{(count == 1 ? "" : $" {count}")}".Trim(), separator);
                count += 1;
            } while (await exists(slug));

            return slug;
        }

        // URL Slugify algorithm in C#?
        // source: https://stackoverflow.com/questions/2920744/url-slugify-algorithm-in-c/2921135#2921135
        public static string GenerateSlug(string input, string separator = "-")
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (separator == null)
                throw new ArgumentNullException(nameof(input));

            static string RemoveDiacritics(string text)
            {
                var normalizedString = text.Normalize(NormalizationForm.FormD);
                var stringBuilder = new StringBuilder();

                foreach (var c in normalizedString)
                {
                    var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                    if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    {
                        stringBuilder.Append(c);
                    }
                }

                return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
            }

            // remove all diacritics.
            input = RemoveDiacritics(input);

            // Remove everything that's not a letter, number, hyphen, dot, whitespace or underscore.
            input = Regex.Replace(input, @"[^a-zA-Z0-9\-\.\s_]", string.Empty, RegexOptions.Compiled).Trim();

            // replace symbols with a hyphen.
            input = Regex.Replace(input, @"[\-\.\s_]", separator, RegexOptions.Compiled);

            // replace double occurrences of hyphen.
            input = Regex.Replace(input, @"(-){2,}", "$1", RegexOptions.Compiled).Trim('-');

            return input;
        }

        public static string GenerateHash(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var byteValue = Encoding.UTF8.GetBytes(input);
            var byteHash = SHA256.HashData(byteValue);
            return Convert.ToBase64String(byteHash);
        }

        public const string NATURAL_NUMERIC_CHARS = "123456789";
        public const string WHOLE_NUMERIC_CHARS = "0123456789";
        public const string LOWER_ALPHA_CHARS = "abcdefghijklmnopqrstuvwyxz";
        public const string UPPER_ALPHA_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        // How can I generate random alphanumeric strings?
        // source: https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings
        public static string GenerateText(int size, string characters)
        {
            if (characters == null)
                throw new ArgumentNullException(nameof(characters));

            var charArray = characters.ToCharArray();
            byte[] data = new byte[4 * size];
            using (var crypto = RandomNumberGenerator.Create())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(size);
            for (int i = 0; i < size; i++)
            {
                var rnd = BitConverter.ToUInt32(data, i * 4);
                var idx = rnd % charArray.Length;

                result.Append(charArray[idx]);
            }

            return result.ToString();
        }

        public static string GenerateStamp()
        {
            static byte[] Generate128BitsOfRandomEntropy()
            {
                var randomBytes = new byte[16]; // 16 Bytes will give us 128 bits.
                using (var rngCsp = RandomNumberGenerator.Create())
                {
                    // Fill the array with cryptographically secure random bytes.
                    rngCsp.GetBytes(randomBytes);
                }
                return randomBytes;
            }

            return new Guid(Generate128BitsOfRandomEntropy()).ToString().Replace("-", "", StringComparison.Ordinal);
        }
    }
}