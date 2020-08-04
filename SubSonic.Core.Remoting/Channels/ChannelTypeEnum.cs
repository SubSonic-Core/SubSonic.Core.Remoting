using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.Remoting.Channels
{
    [Flags]
    internal enum ChannelTypeEnum
    {
        Unknown = 0,
        Sender = 1,
        Receiver = 2
    }
}
