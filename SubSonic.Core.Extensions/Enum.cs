using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core
{
    public static partial class Extensions
    {
        public static TEnum Parse<TEnum>(this string source)
            where TEnum: struct
        {
            if(Enum.TryParse(source, true, out TEnum result))
            {
                return result;
            }
            return default;
        }
    }
}
