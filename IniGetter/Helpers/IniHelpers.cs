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
        public static IniItem GetIniItem(this IEnumerable<IniItem> itemList, string section, string key)
        {
            return itemList.FirstOrDefault(x => x.Section == section && x.Key == key);
        }

        /// <summary>
        /// Gets all key names for a given section.
        /// </summary>
        /// <param name="itemList">The collection of INI items.</param>
        /// <param name="section">The section name.</param>
        /// <returns>An array of key names.</returns>
        public static string[] GetKeys(this IEnumerable<IniItem> itemList, string section)
        {
            return itemList.Where(x => x.Section == section).Select(x => x.Key).Distinct().ToArray();
        }

        /// <summary>
        /// Gets all section names from the collection.
        /// </summary>
        /// <param name="itemList">The collection of INI items.</param>
        /// <returns>An array of section names.</returns>
        public static string[] GetSections(this IEnumerable<IniItem> itemList)
        {
            return itemList.Select(x => x.Section).Distinct().ToArray();
        }

        /// <summary>
        /// Escapes a string for INI file output.
        /// </summary>
        /// <param name="str">The string to escape.</param>
        /// <returns>The escaped string.</returns>
        public static string IniEscaped(this string str)
        {
            string retVal = JsonConvert.ToString(str);
            return (retVal.Contains('\\')) ? retVal : str;
        }

        /// <summary>
        /// Unescapes a string from INI file input.
        /// </summary>
        /// <param name="str">The string to unescape.</param>
        /// <returns>The unescaped string.</returns>
        public static string IniUnescaped(this string str)
        {
            Regex checkQuoted = new Regex(@"""[^""\\]*(?:\\.[^""\\]*)*""");
            Match matchResult = checkQuoted.Match(str);
            if (matchResult.Success)
            {
                return JsonConvert.DeserializeObject<string>(matchResult.Value);
            }
            else
            {
                return str;
            }
        }

        /// <summary>
        /// Splits a string into lines, trimming each line.
        /// </summary>
        /// <param name="str">The string to split.</param>
        /// <returns>An array of trimmed lines.</returns>
        public static string[] ToLines(this string str)
        {
            List<string> retVal = new List<string>();
            using (StringReader sr = new StringReader(str))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    retVal.Add(line.Trim());
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
            Regex regex = new Regex(@"^[A-Za-z0-9_\-\!\ \.\@\&\^\$]+$");

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
                switch (str.ToLower())
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
                            Regex regex = new Regex(@"^-?[0-9][0-9,\.]*$");
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