using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization
{
    public sealed class TypeLoadExceptionHolder
    {
        public TypeLoadExceptionHolder(string typeName)
        {
            TypeName = typeName;
        }

        public readonly string TypeName;
    }
}
