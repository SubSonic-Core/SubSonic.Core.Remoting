using SubSonic.Core.Remoting.Serialization.Binary;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization
{
    public sealed class FormatterHelper
    {
        public FormatterTypeStyle TypeFormat { get; set; }
        public FormatterAssemblyStyle AssemblyFormat { get; set; }
        public TypeFilterLevel SecurityLevel { get; set; }
        public SerializerType SerializerType { get; set; }
    }
}
