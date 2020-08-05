using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace SubSonic.Core.Remoting.Serialization.Binary
{
    public interface IStreamable
    {
        void Read(BinaryParser input);
        void Write(BinaryFormatterWriter output);
    }
}
