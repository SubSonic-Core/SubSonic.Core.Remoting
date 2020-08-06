using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public sealed class SerializationObjectInfo
    {
        public int _objectInfoIdCount = 1;
        public SerializationStack _oiPool = new SerializationStack("SerializationObjectInfo Pool");

        public Dictionary<Type, SerializationObjectInfoCache> SeenBeforeTable { get; } = new Dictionary<Type, SerializationObjectInfoCache>();
    }
}
