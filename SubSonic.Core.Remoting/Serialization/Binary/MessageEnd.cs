using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    internal sealed class MessageEnd : IStreamable
    {
        internal MessageEnd()
        {
        }

        public void Read(BinaryParser input)
        {
        }

        public void Write(BinaryFormatterWriter output)
        {
            output.WriteByte(11);
        }
    }
}
