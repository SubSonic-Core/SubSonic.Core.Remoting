using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting.Proxies
{
    [Flags]
    internal enum RealProxyFlags
    {
        None,
        RemotingProxy,
        Initialized
    }
}
