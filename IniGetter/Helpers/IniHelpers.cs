using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace IniGetter.Helpers
{
    internal static class IniHelpers
    {
        public static IniItem GetIniItem(this IEnumerable<IniItem> itemList, string section, string key)
        {
            return itemList.FirstOrDefault(x => x.Section == section && x.Key == key);
        }

        public static string[] GetKeys(this IEnumerable<IniItem> itemList, string section)
        {
            return itemList.Where(x => x.Section == section).Select(x => x.Key).Distinct().ToArray();
        }

        public static string[] GetSections(this IEnumerable<IniItem> itemList)
        {
            return itemList.Select(x => x.Section).Distinct().ToArray();
        }

        public static string IniEscaped(this string str)
        {
            string retVal = JsonConvert.ToString(str);
            return (retVal.Contains('\\')) ? retVal : str;
        }

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

        public static string[] ToLines(this string str)
        {
            List<string> retVal = new List<string>();
            using (StringReader sr = new StringReader(str))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    // do something
                    retVal.Add(line.Trim());
                }
            }
            return retVal.ToArray();
        }

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