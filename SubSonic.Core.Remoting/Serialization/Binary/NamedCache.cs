using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    internal sealed class NameCache
    {
        private readonly static ConcurrentDictionary<string, object> s_hash;

        static NameCache()
        {
            s_hash = new ConcurrentDictionary<string, object>();
        }

        public object GetCachedValue(string name)
        {
            if (s_hash.TryGetValue(name, out object value))
            {
                return value;
            }
            return default;
        }

        public void SetCachedValue(string name, object value)
        {
            s_hash[name] = value;
        }
    }
}
