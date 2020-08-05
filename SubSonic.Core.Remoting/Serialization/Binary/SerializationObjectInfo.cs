using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public sealed class SerializationObjectInfo
    {
        private readonly Dictionary<Type, SerializationObjectInfo> _seenBeforeTable = new Dictionary<Type, SerializationObjectInfo>();
        public int _objectInfoIdCount = 1;
        public SerializationStack _oiPool = new SerializationStack("SerializationObjectInfo Pool");
    }
}
