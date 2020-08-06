using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public class TypeInformation
    {
        public TypeInformation(string fullTypeName, string assemblyString, bool hasTypeForwardedFrom)
        {
            FullTypeName = fullTypeName;
            AssemblyString = assemblyString;
            HasTypeForwardedFrom = hasTypeForwardedFrom;
        }

        protected TypeInformation(TypeInformation info)
            : this(info.FullTypeName, info.AssemblyString, info.HasTypeForwardedFrom) { }

        public string FullTypeName;

        public string AssemblyString;

        public bool HasTypeForwardedFrom;
    }
}
