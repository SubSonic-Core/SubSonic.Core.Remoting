﻿using SubSonic.Core.Remoting.Serialization.Binary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Text;
using ObjectReader = SubSonic.Core.Remoting.Serialization.Binary.ObjectReader;

namespace SubSonic.Core.Remoting.Serialization
{
    public sealed class BinaryFormatter
        : IFormatter
    {
        private static readonly ConcurrentDictionary<Type, TypeInformation> s_typeNameCache = new ConcurrentDictionary<Type, TypeInformation>();
        public FormatterTypeStyle TypeFormat { get; set; }
        public TypeFilterLevel FilterLevel { get; set; }
        private object[] crossAppDomainArray;
        public FormatterAssemblyStyle AssemblyStyle { get; set; }

        public SerializationBinder Binder { get; set; }
        public StreamingContext Context { get; set; }
        public ISurrogateSelector SurrogateSelector { get; set; }

        public BinaryFormatter()
            : this(null, new StreamingContext(StreamingContextStates.All))
        {

        }

        public BinaryFormatter(ISurrogateSelector selector, StreamingContext context)
        {
            TypeFormat = FormatterTypeStyle.TypesAlways;
            FilterLevel = TypeFilterLevel.Full;
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
            return Deserialize(serializationStream, true);
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

            FormatterHelper fh = new FormatterHelper
            {
                TypeFormat = TypeFormat,
                SerializerType = Binary.SerializerType.Binary,
                AssemblyFormat = AssemblyStyle,
                SecurityLevel = FilterLevel
            };

            ObjectReader reader = new ObjectReader(serializationStream, SurrogateSelector, Context, fh, Binder);
            reader.CrossAppDomainArray = this.crossAppDomainArray;
            return reader.Deserialize(new BinaryParser(serializationStream, reader), check);
        }

        public void Serialize(Stream serializationStream, object graph)
        {
            this.Serialize(serializationStream, graph, true);
        }

        internal void Serialize(Stream serializationStream, object graph, bool check)
        {
            if (serializationStream == null)
            {
                throw new ArgumentNullException("serializationStream");
            }
            FormatterHelper fh = new FormatterHelper
            {
                TypeFormat = TypeFormat,
                SerializerType = Binary.SerializerType.Binary,
                AssemblyFormat = AssemblyStyle,
                SecurityLevel = FilterLevel
            };

            ObjectWriter objectWriter = new ObjectWriter(SurrogateSelector, Context, fh, Binder);
            BinaryFormatterWriter serWriter = new BinaryFormatterWriter(serializationStream, objectWriter, TypeFormat);
            objectWriter.Serialize(graph, serWriter, check);
            crossAppDomainArray = objectWriter._crossAppDomainArray;
        }
    }
}
