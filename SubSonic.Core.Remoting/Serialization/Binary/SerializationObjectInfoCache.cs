using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public sealed class SerializationObjectInfoCache
        : TypeInformation
    {
        public MemberInfo[] MemberInfos;
        public string[] MemberNames;
        public Type[] MemberTypes;

        public SerializationObjectInfoCache(string typeName, string assemblyName, bool hasTypeForwardedFrom)
            : base(typeName, assemblyName, hasTypeForwardedFrom) { }

        public SerializationObjectInfoCache(Type type)
            : base(BinaryFormatter.GetTypeInformation(type)) { }        
    }
}
