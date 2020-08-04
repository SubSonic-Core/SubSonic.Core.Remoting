using System.Collections.Generic;
using System.Linq;

namespace SubSonic.Core
{
    public static partial class Extensions
    {
        public static void AddIfNotExist<TType>(this ICollection<TType> collection, TType element)
        {
            if (!collection.Any(x => x.Equals(element)))
            {
                collection.Add(element);
            }
        }
    }
}
