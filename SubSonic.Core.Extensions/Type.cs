using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SubSonic.Core
{
    public static partial class Extensions
    {
        public static string ToConfigName(this Type type)
        {
            // Do not qualify types from mscorlib/System.Private.CoreLib otherwise calling between process running with different frameworks won't work
            // i.e. "System.String, mscorlib" (.NET FW) != "System.String, System.Private.CoreLib" (.NET CORE)
            if (type.Assembly.GetName().Name == "mscorlib" ||
                type.Assembly.GetName().Name == "System.Private.CoreLib")
                return type.FullName;

            var name = type.AssemblyQualifiedName;
            name = Regex.Replace(name, @", Version=\d+.\d+.\d+.\d+", string.Empty);
            name = Regex.Replace(name, @", Culture=\w+", string.Empty);
            name = Regex.Replace(name, @", PublicKeyToken=\w+", string.Empty);
            return name;
        }
    }
}
