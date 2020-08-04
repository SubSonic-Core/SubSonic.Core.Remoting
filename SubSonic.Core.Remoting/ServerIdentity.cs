using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting
{
    public class ServerIdentity
        : Identity
    {
        public ServerIdentity(MarshalByRefObject byRefObject)
            : base(byRefObject is ContextBoundObject)
        {
            if (!RemotingServices.IsTransparentProxy(byRefObject))
            {

            }
        }

        public Type ServerType { get; protected set; }
    }
}
