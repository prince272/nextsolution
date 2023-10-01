namespace NextSolution.Core.Utilities
{
    public static class StringExtension
    {
        /// <summary>
        /// Trims the specified substring from the end of the input string.
        /// </summary>
        /// <param name="str">The input string to be trimmed.</param>
        /// <param name="trimStr">The substring to remove from the end of the input string.</param>
        /// <param name="repeatTrim">Specifies whether to repeat the trimming process until no more occurrences are found (default is true).</param>
        /// <param name="comparisonType">The type of string comparison to use for finding the substring (default is OrdinalIgnoreCase).</param>
        /// <returns>The trimmed string.</returns>
        public static string TrimEnd(this string str, string trimStr,
                                     bool repeatTrim = true,
                                     StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            return TrimStr(str, trimStr, true, repeatTrim, comparisonType);
        }

        /// <summary>
        /// Trims the specified substring from the start of the input string.
        /// </summary>
        /// <param name="str">The input string to be trimmed.</param>
        /// <param name="trimStr">The substring to remove from the start of the input string.</param>
        /// <param name="repeatTrim">Specifies whether to repeat the trimming process until no more occurrences are found (default is true).</param>
        /// <param name="comparisonType">The type of string comparison to use for finding the substring (default is OrdinalIgnoreCase).</param>
        /// <returns>The trimmed string.</returns>
        public static string TrimStart(this string str, string trimStr,
                                       bool repeatTrim = true,
                                       StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            return TrimStr(str, trimStr, false, repeatTrim, comparisonType);
        }

        /// <summary>
        /// Trims the specified substring from the start and end of the input string.
        /// </summary>
        /// <param name="str">The input string to be trimmed.</param>
        /// <param name="trimStr">The substring to remove from the start and end of the input string.</param>
        /// <param name="repeatTrim">Specifies whether to repeat the trimming process until no more occurrences are found (default is true).</param>
        /// <param name="comparisonType">The type of string comparison to use for finding the substring (default is OrdinalIgnoreCase).</param>
        /// <returns>The trimmed string.</returns>
        public static string Trim(this string str, string trimStr,
                                  bool repeatTrim = true,
                                  StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            return str.TrimStart(trimStr, repeatTrim, comparisonType)
                      .TrimEnd(trimStr, repeatTrim, comparisonType);
        }

        /// <summary>
        /// Trims the specified substring from the start or end of the input string.
        /// </summary>
        /// <param name="str">The input string to be trimmed.</param>
        /// <param name="trimStr">The substring to remove from the start or end of the input string.</param>
        /// <param name="trimEnd">Specifies whether to trim the substring from the end (default is true).</param>
        /// <param name="repeatTrim">Specifies whether to repeat the trimming process until no more occurrences are found (default is true).</param>
        /// <param name="comparisonType">The type of string comparison to use for finding the substring (default is OrdinalIgnoreCase).</param>
        /// <returns>The trimmed string.</returns>
        private static string TrimStr(this string str, string trimStr,
                                     bool trimEnd = true, bool repeatTrim = true,
                                     StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            while (repeatTrim)
            {
                int strLen = str.Length;

                if (trimEnd)
                {
                    if (!str.EndsWith(trimStr, comparisonType))
                        break;

                    int pos = str.LastIndexOf(trimStr, comparisonType);
                    if (pos < 0 || str.Length - trimStr.Length != pos)
                        break;

                    str = str.Substring(0, pos);
                }
                else
                {
                    if (!str.StartsWith(trimStr, comparisonType))
                        break;

                    str = str.Substring(trimStr.Length, str.Length - trimStr.Length);
                }

                if (str.Length == strLen)
                    break;
            }

            return str;
        }
    }

}
