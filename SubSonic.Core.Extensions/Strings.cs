using System;
using System.Globalization;

namespace SubSonic.Core
{
    public static partial class Extensions
    {
        public static bool IsNullOrEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }

        public static bool IsNotNullOrEmpty(this string source)
        {
            return !string.IsNullOrEmpty(source);
        }

        public static string Format(this string source, params object[] arguments)
        {
            return source.Format(CultureInfo.InvariantCulture, arguments);
        }

        public static string Format(this string source, IFormatProvider provider, params object[] arguments)
        {
            return string.Format(provider, source, arguments);
        }
    }
}
