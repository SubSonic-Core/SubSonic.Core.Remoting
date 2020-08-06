using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    internal sealed class BinaryCrossAppDomainMap : IStreamable
    {
        internal int _crossAppDomainArrayIndex;

        public void Read(BinaryParser input)
        {
            this._crossAppDomainArrayIndex = input.ReadInt32();
        }

        public void Write(BinaryFormatterWriter output)
        {
            throw new NotImplementedException();
        }
    }
}
