using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography.X509Certificates;

namespace IniGetter
{
    class IniItem
    {
        private string _section;
        private string _key;
        private string _value;
        private string _comment;

        public string Section { get => _section; set => _section = value; }
        public string Key { get => _key; set => _key = value; }
        public string Value { get => _value; set => this._value = value; }
        public string Comment { get => _comment; set => _comment = value; }
    }
}
