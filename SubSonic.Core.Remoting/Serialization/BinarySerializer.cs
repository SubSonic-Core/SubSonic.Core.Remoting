using ServiceWire;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization
{
    public class BinarySerializer
        : ISerializer
    {
        private readonly IFormatter formatter;

        public BinarySerializer()
        {
            formatter = new BinaryFormatter();
        }

        public T Deserialize<T>(byte[] bytes)
        {
            if (bytes != null && bytes.Length > 0)
            {
                using (var stream = new MemoryStream(bytes))
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
                using (var stream = new MemoryStream(bytes))
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

            using(var stream = new MemoryStream())
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

            using (var stream = new MemoryStream())
            {
                var type = typeConfigName.ToType();
                var objT = Convert.ChangeType(obj, type);
                formatter.Serialize(stream, objT);
                return stream.ToArray();
            }
        }
    }
}
