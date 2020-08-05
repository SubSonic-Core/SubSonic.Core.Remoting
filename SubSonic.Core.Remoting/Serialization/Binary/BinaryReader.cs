using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public sealed class BinaryReader
    {
        private Stream stream;
        private ISurrogateSelector surrogates;
        private StreamingContext context;
        private ObjectManager objectManager;
        private FormatterHelper formatterHelper;
        private SerializationBinder binder;
        private long topId;
        private bool isSimpleAssembly;
        private object topObject;
        private 
    }
}
