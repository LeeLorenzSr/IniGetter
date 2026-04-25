using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace IniGetter.Helpers
{
    /// <summary>
    /// Provides extension methods for working with INI file items and strings.
    /// </summary>
    internal static class IniHelpers
    {
        /// <summary>
        /// Gets the <see cref="IniItem"/> with the specified section and key.
        /// </summary>
        /// <param name="itemList">The collection of INI items.</param>
        /// <param name="section">The section name.</param>
        /// <param name="key">The key name.</param>
        /// <returns>The matching <see cref="IniItem"/>, or null if not found.</returns>
        public static IniItem GetIniItem(this IEnumerable<IniItem> itemList, string section, string key) =>
            itemList.FirstOrDefault(x => x.Section == section && x.Key == key);

        /// <summary>
        /// Gets all key names for a given section.
        /// </summary>
        /// <param name="itemList">The collection of INI items.</param>
        /// <param name="section">The section name.</param>
        /// <returns>An array of key names.</returns>
        public static string[] GetKeys(this IEnumerable<IniItem> itemList, string section) =>
            itemList.Where(x => x.Section == section).Select(x => x.Key).Distinct().ToArray();

        /// <summary>
        /// Gets all section names from the collection.
        /// </summary>
        /// <param name="itemList">The collection of INI items.</param>
        /// <returns>An array of section names.</returns>
        public static string[] GetSections(this IEnumerable<IniItem> itemList) =>
            itemList.Select(x => x.Section).Distinct().ToArray();

        /// <summary>
        /// Escapes a string for INI file output.
        /// </summary>
        /// <param name="str">The string to escape.</param>
        /// <returns>The escaped string.</returns>
        public static string IniEscaped(this string str)
        {
            if (str == null)
            {
                return null;
            }

            var retVal = JsonConvert.ToString(str);
            return str.Length != str.Trim().Length || str.Contains(";") || str.Contains("#") || retVal.Contains('\\')
                ? retVal
                : str;
        }

        /// <summary>
        /// Unescapes a string from INI file input.
        /// </summary>
        /// <param name="str">The string to unescape.</param>
        /// <returns>The unescaped string.</returns>
        public static string IniUnescaped(this string str)
        {
            if (str == null)
            {
                return null;
            }

            return IsEntireQuotedString(str)
                ? JsonConvert.DeserializeObject<string>(str)
                : str;
        }

        /// <summary>
        /// Attempts to extract a quoted INI value and optional trailing comment.
        /// </summary>
        /// <param name="str">The raw INI value string.</param>
        /// <param name="allowPoundComment">Whether pound comments are enabled.</param>
        /// <param name="valuePart">The extracted quoted value.</param>
        /// <param name="commentPart">The extracted comment text without the comment marker.</param>
        /// <returns>True if the input starts with a complete quoted value and has no trailing text other than whitespace or a comment.</returns>
        public static bool TryExtractQuotedValue(this string str, bool allowPoundComment, out string valuePart, out string commentPart)
        {
            valuePart = str;
            commentPart = null;

            if (string.IsNullOrEmpty(str) || str[0] != '"')
            {
                return false;
            }

            int closingQuoteIndex = FindClosingQuoteIndex(str);
            if (closingQuoteIndex < 0)
            {
                return false;
            }

            valuePart = str.Substring(0, closingQuoteIndex + 1);

            var suffix = str.Substring(closingQuoteIndex + 1);
            if (suffix.Trim().Length == 0)
            {
                return true;
            }

            var trimmedSuffix = suffix.TrimStart();
            if (!trimmedSuffix.StartsWith(";") && !(allowPoundComment && trimmedSuffix.StartsWith("#")))
            {
                valuePart = str;
                return false;
            }

            commentPart = trimmedSuffix.Substring(1).Trim();
            return true;
        }

        private static bool IsEntireQuotedString(string str)
        {
            if (string.IsNullOrEmpty(str) || str[0] != '"')
            {
                return false;
            }

            return FindClosingQuoteIndex(str) == str.Length - 1;
        }

        private static int FindClosingQuoteIndex(string str)
        {
            for (int i = str.Length - 1; i > 0; i--)
            {
                if (str[i] != '"')
                {
                    continue;
                }

                int backslashCount = 0;
                for (int j = i - 1; j >= 0 && str[j] == '\\'; j--)
                {
                    backslashCount++;
                }

                if (backslashCount % 2 == 0)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Splits a string into lines without trimming each line.
        /// </summary>
        /// <param name="str">The string to split.</param>
        /// <returns>An array of lines.</returns>
        public static string[] ToLines(this string str)
        {
            var retVal = new List<string>();
            using (var sr = new StringReader(str))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    retVal.Add(line);
                }
            }
            return retVal.ToArray();
        }

        /// <summary>
        /// Validates if a string is a valid INI section or key name.
        /// </summary>
        /// <param name="str">The string to validate.</param>
        /// <returns>True if valid, otherwise false.</returns>
        public static bool ValidateName(this string str)
        {
            Regex regex = new Regex(@"^[A-Za-z0-9_\-\! \.\@\&\^\$]+$");

            bool bReturn = false;
            if (str.Length > 0 && regex.IsMatch(str))
            {
                bReturn = true;
            }
            return bReturn;

        }

        /// <summary>
        /// Attempts to robustly parse a string as a boolean value.
        /// </summary>
        /// <param name="str">The string to parse.</param>
        /// <param name="bValue">The parsed boolean value.</param>
        /// <returns>True if parsing was successful, otherwise false.</returns>
        public static bool TryToRobustBoolean(this string str, out bool bValue)
        {
            bool bReturn = false;
            bValue = false;

            if (!string.IsNullOrEmpty(str))
            {
                switch (str.ToLowerInvariant())
                {
                    case "true":
                    case "yes":
                    case "on":
                    case "enabled":
                    case "enable":
                    case "active":
                        bValue = true;
                        bReturn = true;
                        break;
                    case "false":
                    case "no":
                    case "off":
                    case "disabled":
                    case "disable":
                    case "inactive":
                        bValue = false;
                        bReturn = true;
                        break;
                    default:
                        {
                            var regex = new Regex(@"^-?[0-9][0-9,\.]*$");
                            if (regex.IsMatch(str))
                            {
                                if (float.TryParse(str, out float testValue))
                                {
                                    bValue = (testValue != 0);
                                    bReturn = true;
                                }
                            }
                        }
                        break;
                }
            }
            return bReturn;
        }
    }
}