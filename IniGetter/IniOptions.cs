using System;
using System.Collections.Generic;
using System.Text;

namespace IniGetter
{
    public class IniOptions
    {
        public bool ReadOnly = true;
        public bool AllowSave = false;
        public bool CaseSensitive = false;
        public bool PoundComment = true;
        public char NameValueDelimiter = '=';
        public bool IgnoreSpacesInNames = false;
        public bool MultilineSupport = false;
    }
}
