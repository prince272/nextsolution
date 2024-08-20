using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Next_Solution.WebApi.Helpers
{
    public static class TextHelper
    {
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
    }
}
