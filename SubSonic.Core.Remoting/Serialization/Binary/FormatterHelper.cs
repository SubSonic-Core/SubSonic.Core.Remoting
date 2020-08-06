using SubSonic.Core.Remoting.Serialization.Binary;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization
{
    public sealed class FormatterHelper
    {
        internal FormatterTypeStyle TypeFormat;
        internal FormatterAssemblyStyle AssemblyFormat;
        internal TypeFilterLevel SecurityLevel;
        internal SerializerType SerializerType;
    }
}
