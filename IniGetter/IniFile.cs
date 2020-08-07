using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using IniGetter.Helpers;

namespace IniGetter
{
    public class IniFile : IComparable
    {
        private readonly List<IniItem> _iniItems = new List<IniItem>();
        private readonly List<string> _parseWarnings = new List<string>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">Options used to handle the ini file</param>
        public IniFile(IniOptions options = null)
        {
            if (options != null)
            {
                this.Options = options;
            }
            else
            {
                this.Options = new IniOptions();
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filePath">file path to ini file</param>
        /// <param name="options">Options used to handle the ini file</param>
        /// <param name="prefix">Optional prefix appended to section names when parsing file</param>
        public IniFile(string filePath, IniOptions options = null, string prefix = "") : this(options)
        {
            Load(filePath, false, prefix);
        }

        /// <summary>
        /// The last warning generated during the previous operation
        /// </summary>
        public string LastWarning { get; private set; } = "";

        /// <summary>
        /// Options used when creating this instance
        /// </summary>
        public IniOptions Options { get; }

        /// <summary>
        /// A collection of parse warnings that occurred during the previous parse
        /// </summary>
        public string[] ParseWarnings { get => _parseWarnings.ToArray(); }

        public static IniFile operator +(IniFile first, IniFile second)
        {
            IniFile result = new IniFile(first.Options);

            result._iniItems.AddRange(first._iniItems);
            foreach (var item in second._iniItems)
            {
                var oldItem = result._iniItems.GetIniItem(item.Section, item.Key);
                if (oldItem == null)
                {
                    result._iniItems.Add(item);
                }
                else
                {
                    result.SetParseWarning(0, $"Item overwritten [{oldItem.Section}][{oldItem.Key}] value ({oldItem.Value}) overwritten by ({item.Value})");
                    oldItem.Value = item.Value;
                    oldItem.Comment = item.Comment;
                }
            }
            return result;
        }

        public static string UnescapeString(string str)
        {
            return str;
        }

        /// <summary>
        /// Clear all values loaded into this instance
        /// </summary>
        public void Clear()
        {
            _iniItems.Clear();
        }

        /// <summary>
        /// For IComparable. Not efficient, but we should not expect a lot of comparisons
        /// </summary>
        /// <param name="obj">object to compare to</param>
        /// <returns>Comparison value</returns>
        public int CompareTo(object obj)
        {
            if (obj.GetType() == typeof(IniFile))
            {
                return this.ToString().CompareTo(((IniFile)obj).ToString());
            }
            else if (obj.GetType() == typeof(string))
            {
                return this.ToString().CompareTo(obj as string);
            }
            return -1;
        }

        /// <summary>
        /// Get a string value from the loaded INI settings
        /// </summary>
        /// <param name="section">The section for this setting, (Global section will use an empty string)</param>
        /// <param name="key">Name of the setting within the section</param>
        /// <param name="defaultValue">The default string value, if the setting is not present</param>
        /// <returns>Returns the value of the setting as a string</returns>
        public string Get(string section, string key, string defaultValue = null)
        {
            string sReturn = defaultValue;

            var item = _iniItems.GetIniItem(ConvertName(section), ConvertName(key));
            if (item != null)
            {
                sReturn = item.Value;
            }

            return sReturn;
        }

        /// <summary>
        /// Get a boolean value from the loaded INI settings
        /// This is a robust parse of the value:
        ///  - accepting any numeric value other than 0 as true (0 is false)
        ///  - True/False
        ///  - On/Off
        ///  - Yes/No
        ///  - Enable/Disable
        ///  - Enabled/Disabled
        ///  - Active/Inactive
        /// </summary>
        /// <param name="section">The section for this setting, (Global section will use an empty string)</param>
        /// <param name="key">Name of the setting within the section</param>
        /// <param name="defaultValue">The default boolean value, if the setting is not present</param>
        /// <returns>Returns the value of the setting as a boolean</returns>
        public bool Get(string section, string key, bool defaultValue)
        {
            bool bReturn = defaultValue;

            string sVal = Get(section, key);
            if (sVal != null)
            {
                if ( sVal.TryToRobustBoolean(out bool testValue))
                {
                    bReturn = testValue;
                }
            }
            return bReturn;
        }

        /// <summary>
        /// Get a double value from the loaded INI settings
        /// </summary>
        /// <param name="section">The section for this setting, (Global section will use an empty string)</param>
        /// <param name="key">Name of the setting within the section</param>
        /// <param name="defaultValue">The default double value, if the setting is not present</param>
        /// <returns>Returns the value of the setting as a double</returns>
        public double Get(string section, string key, double defaultValue)
        {
            double retVal = defaultValue;
            Regex regex = new Regex(@"^-?[0-9][0-9,\.]+$");

            string sVal = Get(section, key);
            if (sVal != null)
            {
                if (regex.IsMatch(sVal))
                {
                    if (double.TryParse(sVal, out double testValue))
                    {
                        retVal = testValue;
                    }
                }
            }
            return retVal;
        }

        /// <summary>
        /// Get an integer value from the loaded INI settings
        /// </summary>
        /// <param name="section">The section for this setting, (Global section will use an empty string)</param>
        /// <param name="key">Name of the setting within the section</param>
        /// <param name="defaultValue">The default integer value, if the setting is not present</param>
        /// <returns>Returns the value of the setting as an integer</returns>
        public Int64 Get(string section, string key, Int64 defaultValue)
        {
            Int64 retVal = defaultValue;
            Regex regex = new Regex("^[0-9]+$");

            string sVal = Get(section, key);
            if (sVal != null)
            {
                if (regex.IsMatch(sVal))
                {
                    if (Int64.TryParse(sVal, out long testValue))
                    {
                        retVal = testValue;
                    }
                }
            }
            return retVal;
        }

        /// <summary>
        /// Fetches the comment associated with this setting
        /// </summary>
        /// <param name="section">Section name</param>
        /// <param name="key">Key name</param>
        /// <returns>Comment, or empty string if there is none</returns>
        public string GetComment(string section, string key)
        {
            string retVal = string.Empty;

            var item = _iniItems.GetIniItem(ConvertName(section), ConvertName(key));
            if (item != null && string.IsNullOrEmpty(item.Comment))
            {
                retVal = item.Comment;
            }
            return retVal;
        }

        /// <summary>
        /// Get the key names of the loaded values for a section
        /// </summary>
        /// <param name="sectionName">The section name to query for (Empty string or null for global section)</param>
        /// <returns>An array of key names</returns>
        public string[] GetKeyNames(string sectionName)
        {
            return _iniItems.GetKeys(sectionName);
        }

        /// <summary>
        /// Get the section names of the loaded values
        /// </summary>
        /// <returns>An array of section names, including Global as an empty string, if it contains any values</returns>
        public string[] GetSectionNames()
        {
            return _iniItems.GetSections();
        }

        /// <summary>
        /// Load the values from an INI file
        /// If you are merging, it may generate warnings
        /// </summary>
        /// <param name="filePath">Path to the file containing valid INI format content</param>
        /// <param name="mergeFile">If true, merges with any existing values already loaded</param>
        /// <param name="prefix">A string that is prepended to the section names as the settings are loaded</param>
        /// <returns>true if the loading occurs without warnings, false if warnings occur</returns>
        public bool Load(string filePath, bool mergeFile = false, string prefix = "")
        {
            bool bReturn = true;
            string[] iniLines = new string[] { };
            if (!mergeFile)
            {
                Clear();
            }
            try
            {
                ClearParseWarnings();
                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        iniLines = System.IO.File.ReadAllLines(filePath);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Exception occurred: {ex.Message}");
                    }
                }
                else
                {
                    SetParseWarning(0, $"File not found {filePath}");
                    bReturn = false;
                }
            }
            catch (Exception ex)
            {
                SetParseWarning(0, $"Unable to retrieve file [{filePath}] contents: {ex.Message}");
                bReturn = false;
            }
            if (bReturn)
            {
                bReturn = ParseFromLines(iniLines, prefix);
            }
            return bReturn;
        }

        /// <summary>
        /// Load the values from an INI formated string
        /// If you are merging, it may generate warnings
        /// </summary>
        /// <param name="data">String containing valid INI format content</param>
        /// <param name="mergeFile">If true, merges with any existing values already loaded</param>
        /// <param name="prefix">A string that is prepended to the section names as the settings are loaded</param>
        /// <returns>true if the loading occurs without warnings, false if warnings occur</returns>
        public bool LoadFromContent(string data, bool mergeFile = false, string prefix = "")
        {
            if (!mergeFile)
            {
                Clear();
            }
            string[] iniLines = data.ToLines();
            ClearParseWarnings();
            return ParseFromLines(iniLines, prefix);
        }

        /// <summary>
        /// Saves the settings loaded in this instance to a file
        /// </summary>
        /// <param name="filePath">Path of file to write out</param>
        /// <returns>true if the save occurs without issues, false if an error occurs</returns>
        public bool Save(string filePath)
        {
            ClearLastWarning();
            bool bReturn = false;

            try
            {
                System.IO.File.WriteAllText(filePath, this.ToString());
                bReturn = true;
            }
            catch (Exception ex)
            {
                SetLastWarning($"Error occurred saving ini file [{filePath}]: {ex.Message}");
            }

            return bReturn;
        }

        /// <summary>
        /// Set a setting in this INI instance
        /// </summary>
        /// <param name="section">Section name (Empty string if global)</param>
        /// <param name="key">Key name of the setting</param>
        /// <param name="value">Value of the setting</param>
        /// <param name="comment">Optional end of line comment</param>
        /// <returns>True if it overwrote an existing value, False if it is a new entry</returns>
        public bool Set(string section, string key, string value, string comment = null)
        {
            bool bReturn = false;

            var item = _iniItems.GetIniItem(ConvertName(section), ConvertName(key));
            if (item != null)
            {
                item.Value = value;
                item.Comment = comment;
            }
            else
            {
                var newItem = new IniItem()
                {
                    Section = ConvertName(section),
                    Key = ConvertName(key),
                    Value = value,
                    Comment = comment
                };
                _iniItems.Add(newItem);
            }
            return bReturn;
        }
        /// <summary>
        /// Outputs a valid INI formatted string, based on the contents of this instance
        /// </summary>
        /// <returns>INI-formatted string</returns>
        public override string ToString()
        {
            bool bInsertLine = false;
            StringBuilder sbFileContent = new StringBuilder();
            string[] sections = _iniItems.GetSections();
            Array.Sort(sections, StringComparer.InvariantCulture);
            foreach (string currentSection in sections)
            {
                if (bInsertLine)
                {
                    sbFileContent.AppendLine("");
                }
                else
                {
                    bInsertLine = true;
                }
                if (!string.IsNullOrEmpty(currentSection))
                {
                    sbFileContent.AppendLine($"[{currentSection}]");
                }
                string[] keys = _iniItems.GetKeys(currentSection);
                Array.Sort(keys, StringComparer.InvariantCulture);
                foreach (string currentKey in keys)
                {
                    var item = _iniItems.GetIniItem(currentSection, currentKey);
                    if (item != null)
                    {
                        sbFileContent.Append($"{item.Key}={item.Value.IniEscaped()}");
                        if (string.IsNullOrEmpty(item.Comment))
                        {
                            sbFileContent.AppendLine();
                        }
                        else
                        {
                            sbFileContent.AppendLine($" ; {item.Comment}");
                        }
                    }
                    else
                    {
                        // Warning, item not found - this should never happen
                        SetLastWarning($"Unable to retrieve item {currentKey} in section [{currentSection}]");
                    }
                }
            }
            return sbFileContent.ToString();
        }
        private void ClearLastWarning()
        {
            LastWarning = "";
        }

        private void ClearParseWarnings()
        {
            ClearLastWarning();
            _parseWarnings.Clear();
        }

        private string ConvertName(string name)
        {
            if ( string.IsNullOrEmpty(name) )
            {
                name = String.Empty;
            }

            string retVal = name;

            if (Options.IgnoreSpacesInNames)
            {
                retVal = retVal.Replace(" ", "");
            }
            if (!Options.CaseSensitive)
            {
                retVal = retVal.ToLower();
            }
            return retVal;
        }

        private bool IsComment(char ch)
        {
            return (ch == ';') || (Options.PoundComment && (ch == '#'));
        }

        private bool ParseFromLines(string[] lines, string prefix)
        {
            string currentSection = "";
            bool bReturn = false;
            bool bMultiLine = false;
            int currentLineNumber = 1;
            string previousLine = "";

            foreach (string line in lines)
            {
                string workLine = line.Trim();
                if (Options.MultilineSupport)
                {
                    if (bMultiLine)
                    {
                        workLine = previousLine + workLine;
                        bMultiLine = false;
                    }
                    if (!String.IsNullOrEmpty(workLine) && workLine[workLine.Length-1] == '\\')
                    {
                        // Add in next line
                        bMultiLine = true;
                        previousLine = workLine.Substring(0,workLine.Length-1);
                    }

                }
                if (!bMultiLine && !string.IsNullOrEmpty(workLine))
                {
                    currentSection = ParseLine(currentLineNumber, workLine, currentSection, prefix);
                }
                currentLineNumber++;
            }
            // Handle case where multi-line slash is present on last line
            if (bMultiLine)
            {
                ParseLine(currentLineNumber, previousLine, currentSection, prefix);
            }
            return bReturn;
        }

        private string ParseLine(int currentLineNumber, string workLine, string currentSection, string prefix)
        {
            string commentPart = null;
            if (!IsComment(workLine[0]))
            {
                string tempLine = workLine.Trim();
                if (tempLine[0] == '[') // Section name
                {
                    int endPos = tempLine.IndexOf(']');
                    if (endPos > 1)
                    {
                        string sectionName = ConvertName(prefix + tempLine.Substring(1,endPos-1).Trim());
                        if (sectionName.ValidateName())
                        {
                            currentSection = sectionName;
                        }
                        else
                        {
                            SetParseWarning(currentLineNumber, $"Invalid section name");
                        }
                    }
                    else
                    {
                        SetParseWarning(currentLineNumber, $"Malformed section header");
                    }
                }
                else
                {
                    // Name value pair...
                    int endPos = workLine.IndexOf(Options.NameValueDelimiter);
                    if (endPos > 0)
                    {
                        string keyName = ConvertName(workLine.Substring(0, endPos).Trim());
                        if (keyName.ValidateName())
                        {
                            Regex checkQuoted = new Regex(@"""[^""\\]*(?:\\.[^""\\]*)*""");

                            string valueCheck = workLine.Substring(endPos + 1).Trim();
                            Match matchResult = checkQuoted.Match(valueCheck);
                            if (!matchResult.Success)
                            {
                                // Check for comments
                                int commentCheck = valueCheck.IndexOf(';');
                                if ( commentCheck > -1 )
                                {
                                    commentPart = valueCheck.Substring(commentCheck + 1).Trim();
                                    valueCheck = valueCheck.Substring(0,commentCheck).Trim();
                                }
                                else
                                {
                                    commentCheck = valueCheck.IndexOf('#');
                                    if (commentCheck > -1)
                                    {
                                        commentPart = valueCheck.Substring(commentCheck + 1).Trim();
                                        valueCheck = valueCheck.Substring(0, commentCheck).Trim();
                                    }
                                }
                            }
                            string valuePart = valueCheck.IniUnescaped();
                            var oldItem = _iniItems.GetIniItem(ConvertName(currentSection), ConvertName(keyName));
                            if (oldItem != null)
                            {
                                SetParseWarning(currentLineNumber, $"Item overwritten [{oldItem.Section}][{oldItem.Key}] value ({oldItem.Value}) overwritten by ({valuePart})");
                                oldItem.Value = valuePart;
                                oldItem.Comment = commentPart.Trim(); 
                            }
                            else
                            {
                                var newItem = new IniItem()
                                {
                                    Section = currentSection,
                                    Key = keyName,
                                    Value = valuePart                                    
                                };
                                if (!String.IsNullOrEmpty(commentPart))
                                {
                                    newItem.Comment = commentPart.Trim();
                                }
                                _iniItems.Add(newItem);
                            }
                        }
                    }
                    else
                    {
                        SetParseWarning(currentLineNumber, $"malformed name value pair line");
                    }
                }
            }
            return currentSection;
        }
        private void SetLastWarning(string warning)
        {
            LastWarning = warning;
        }

        private void SetParseWarning(int line, string warning)
        {
            string warningText = $"Line {line} : {warning}";
            _parseWarnings.Add(warningText);
            SetLastWarning(warningText);
        }
    }
}