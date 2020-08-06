using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    internal sealed class BinaryAssembly : IStreamable
    {
        internal int _assemId;
        internal string _assemblyString;

        internal BinaryAssembly()
        {
        }

        public void Read(BinaryParser input)
        {
            this._assemId = input.ReadInt32();
            this._assemblyString = input.ReadString();
        }

        internal void Set(int assemId, string assemblyString)
        {
            this._assemId = assemId;
            this._assemblyString = assemblyString;
        }

        public void Write(BinaryFormatterWriter output)
        {
            output.WriteByte(12);
            output.WriteInt32(this._assemId);
            output.WriteString(this._assemblyString);
        }
    }
}
