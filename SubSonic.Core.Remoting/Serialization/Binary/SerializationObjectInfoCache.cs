using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public sealed class SerializationObjectInfoCache
        : TypeInformation
    {
        public MemberInfo[] MemberInfos { get; set; }
        public string[] MemberNames { get; set; }
        public Type[] MemberTypes { get; set; }

        public SerializationObjectInfoCache(string typeName, string assemblyName, bool hasTypeForwardedFrom)
            : base(typeName, assemblyName, hasTypeForwardedFrom) { }

        public SerializationObjectInfoCache(Type type)
            : base(BinaryFormatter.GetTypeInformation(type)) { }        
    }
}
