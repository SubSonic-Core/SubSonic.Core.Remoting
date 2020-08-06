using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SubSonic.Core
{
    public static partial class Utilities
    {
        public static TType Cast<TType>(object value)
        {
            if (value is TType success)
            {
                return success;
            }
            return (TType)Convert.ChangeType(value, typeof(TType), CultureInfo.InvariantCulture);
        }
    }
}
