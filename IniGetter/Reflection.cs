using System;
using System.Reflection;

namespace IniGetter
{
    /// <summary>
    /// Handle the reflection differences between the full .net framework and
    /// what is provided by netstandard
    /// </summary>
    public static class Reflection
    {
        public static Assembly GetAssembly(this Type type)
        {
#if (NET35 || NET40 || NET45 || NET451 || NET46 || NET461)
            return type.Assembly;
#else
            return type.GetTypeInfo().Assembly;
#endif
        }
    }
}
