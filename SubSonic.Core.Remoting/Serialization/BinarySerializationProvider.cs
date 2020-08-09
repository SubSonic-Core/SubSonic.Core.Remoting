using ServiceWire;
using System;
using System.IO;
using System.Runtime.Serialization;

namespace SubSonic.Core.Remoting.Serialization
{
    public class BinarySerializationProvider
        : ISerializationProvider
    {
        private readonly IFormatter formatter;

        public BinarySerializationProvider()
            : this(new BinaryFormatter()) { }

        public BinarySerializationProvider(IFormatter binaryFormatter)
        {
            formatter = binaryFormatter ?? throw new ArgumentNullException(nameof(binaryFormatter));
        }

        public T Deserialize<T>(byte[] bytes)
        {
            if (bytes != null && bytes.Length > 0)
            {
#pragma warning disable IDE0063 // Use simple 'using' statement
                using (var stream = new MemoryStream(bytes))
#pragma warning restore IDE0063 // Use simple 'using' statement
                {
                    if (formatter.Deserialize(stream) is T success)
                    {
                        return success;
                    }
                }
            }
            return default;
        }

        public object Deserialize(byte[] bytes, string typeConfigName)
        {
            if (null == typeConfigName)
            {
                throw new ArgumentNullException(nameof(typeConfigName));
            }

            var type = typeConfigName.ToType();

            if (typeConfigName != null && (bytes != null && bytes.Length > 0))
            {
#pragma warning disable IDE0063 // Use simple 'using' statement
                using (var stream = new MemoryStream(bytes))
#pragma warning restore IDE0063 // Use simple 'using' statement
                {
                    var obj = formatter.Deserialize(stream);

                    return Convert.ChangeType(obj, type);
                }
            }
            return type.GetDefault();
        }

        public byte[] Serialize<T>(T obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

#pragma warning disable IDE0063 // Use simple 'using' statement
            using (var stream = new MemoryStream())
#pragma warning restore IDE0063 // Use simple 'using' statement
            {
                formatter.Serialize(stream, obj);

                return stream.ToArray();
            }
        }

        public byte[] Serialize(object obj, string typeConfigName)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

#pragma warning disable IDE0063 // Use simple 'using' statement
            using (var stream = new MemoryStream())
#pragma warning restore IDE0063 // Use simple 'using' statement
            {
                var type = typeConfigName.ToType();
                var objT = Convert.ChangeType(obj, type);
                formatter.Serialize(stream, objT);
                return stream.ToArray();
            }
        }
    }
}
