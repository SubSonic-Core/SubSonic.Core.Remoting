using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    internal sealed class BinaryCrossAppDomainAssembly : IStreamable
    {
        internal int _assemId;
        internal int _assemblyIndex;

        internal BinaryCrossAppDomainAssembly()
        {
        }

        public void Read(BinaryParser input)
        {
            this._assemId = input.ReadInt32();
            this._assemblyIndex = input.ReadInt32();
        }

        public void Write(BinaryFormatterWriter output)
        {
            throw new NotImplementedException();
        }
    }
}
