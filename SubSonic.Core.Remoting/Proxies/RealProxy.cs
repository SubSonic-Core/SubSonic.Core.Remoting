using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace SubSonic.Core.Remoting.Proxies
{
    public abstract class RealProxy
    {
        private object tp;
        private object identity;
        private MarshalByRefObject serverObject;
        private RealProxyFlags flags;
        private static IntPtr _defaultStub = GetDefaultStub();
        private static IntPtr _defaultStubValue = new IntPtr(-1);
        private static object _defaultStubData = _defaultStubValue;

        protected RealProxy()
        {

        }

        [SecurityCritical]
        protected RealProxy(Type classToProxy)
            : this(classToProxy, IntPtr.Zero, null)
        {

        }

        [SecurityCritical]
        protected RealProxy(Type classToProxy, IntPtr stub, object stubData)
        {
            if (!classToProxy.IsMarshalByRef && !classToProxy.IsInterface)
            {
                throw new ArgumentException(RemotingResources.NotRemotableByReference.Format(classToProxy));
            }

            if (IntPtr.Zero == stub)
            {

            }

            tp = RemotingServices.CreateTransparentProxy(this, classToProxy, stub, stubData);

            if (this is RemotingProxy)
            {
                flags |= RealProxyFlags.RemotingProxy;
            }

        }
    }
}
