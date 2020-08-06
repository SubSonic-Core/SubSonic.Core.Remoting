using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    internal sealed class BinaryCrossAppDomainString : IStreamable
    {
        internal int _objectId;
        internal int _value;

        internal BinaryCrossAppDomainString()
        {
        }

        public void Read(BinaryParser input)
        {
            this._objectId = input.ReadInt32();
            this._value = input.ReadInt32();
        }

        public void Write(BinaryFormatterWriter output)
        {
            throw new NotImplementedException();
        }
    }
}
