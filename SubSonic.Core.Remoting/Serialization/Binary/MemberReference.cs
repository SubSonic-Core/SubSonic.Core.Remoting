using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    internal sealed class MemberReference : IStreamable
    {
        internal int _idRef;

        internal MemberReference()
        {
        }

        public void Read(BinaryParser input)
        {
            this._idRef = input.ReadInt32();
        }

        internal void Set(int idRef)
        {
            this._idRef = idRef;
        }

        public void Write(BinaryFormatterWriter output)
        {
            output.Write((byte)9);
            output.Write(this._idRef);
        }
    }
}
