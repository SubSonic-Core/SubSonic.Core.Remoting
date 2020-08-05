using SubSonic.Core.Remoting.Serialization.Binary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Text;
using BinaryReader = SubSonic.Core.Remoting.Serialization.Binary.BinaryReader;

namespace SubSonic.Core.Remoting.Serialization
{
    public sealed class BinaryFormatter
        : IFormatter
    {
        private static readonly ConcurrentDictionary<Type, TypeInformation> s_typeNameCache = new ConcurrentDictionary<Type, TypeInformation>();
        private FormatterTypeStyle typeFormat;
        private TypeFilterLevel securityLevel;
        private FormatterAssemblyStyle assemblyStyle;

        public SerializationBinder Binder { get; set; }
        public StreamingContext Context { get; set; }
        public ISurrogateSelector SurrogateSelector { get; set; }

        public BinaryFormatter()
            : this(null, new StreamingContext(StreamingContextStates.All))
        {

        }

        public BinaryFormatter(ISurrogateSelector selector, StreamingContext context)
        {
            typeFormat = FormatterTypeStyle.TypesAlways;
            securityLevel = TypeFilterLevel.Full;
            SurrogateSelector = selector;
            Context = context;
        }

        public static TypeInformation GetTypeInformation(Type type)
        {
            return s_typeNameCache.GetOrAdd(type, delegate (Type t) {
                bool flag;
                return new TypeInformation(FormatterServices.GetClrTypeFullName(t), FormatterServices.GetClrAssemblyName(t, out flag), flag);
            });
        }

        public object Deserialize(Stream serializationStream)
        {
            throw new NotImplementedException();
        }

        internal object Deserialize(Stream serializationStream, bool check)
        {
            if (serializationStream == null)
            {
                throw new ArgumentNullException(nameof(serializationStream));
            }
            if (serializationStream.CanSeek && (serializationStream.Length == 0))
            {
                throw new SerializationException();
            }

            FormatterHelper fh = new FormatterHelper();

            fh.TypeFormat = typeFormat;
            fh.SerializerType = Binary.SerializerType.Binary;
            fh.AssemblyFormat = assemblyStyle;
            fh.SecurityLevel = securityLevel;
            BinaryReader reader = new BinaryReader(serializationStream, SurrogateSelector, Context, fh, Binder);
        }

        public void Serialize(Stream serializationStream, object graph)
        {
            throw new NotImplementedException();
        }
    }
}
