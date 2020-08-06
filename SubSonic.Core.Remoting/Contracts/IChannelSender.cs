using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting.Contracts
{
    public interface IChannelSender
    {
        Uri ChannelUri { get; }
    }
}
