using System;
using System.Collections.Generic;
using System.Text;

namespace IniGetter
{
    /// <summary>
    /// Represents options for handling INI files.
    /// </summary>
    public class IniOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the INI file is read-only.
        /// </summary>
        public bool ReadOnly { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether saving is allowed.
        /// </summary>
        public bool AllowSave { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether section and key names are case sensitive.
        /// </summary>
        public bool CaseSensitive { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether '#' is treated as a comment character.
        /// </summary>
        public bool PoundComment { get; set; } = true;

        /// <summary>
        /// Gets or sets the character used to delimit names and values.
        /// </summary>
        public char NameValueDelimiter { get; set; } = '=';

        /// <summary>
        /// Gets or sets a value indicating whether spaces in names are ignored.
        /// </summary>
        public bool IgnoreSpacesInNames { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether multiline values are supported.
        /// </summary>
        public bool MultilineSupport { get; set; } = false;
    }
}
