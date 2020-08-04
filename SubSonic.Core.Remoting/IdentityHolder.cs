using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SubSonic.Core.Remoting
{
    public sealed class IdentityHolder
    {
        private static object s_identityHolderLock = new object();
        private static Dictionary<string, Identity> URITable = new Dictionary<string, Identity>();

        private IdentityHolder()
        {

        }

        [SecurityCritical]
        public static Identity ResolveIdentity(Uri uri)
        {
            Identity identity = null;
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            bool lockTaken = false;

            try
            {
                Monitor.Enter(s_identityHolderLock, ref lockTaken);

                if (URITable.ContainsKey(uri.AbsoluteUri))
                {
                    identity = URITable[uri.AbsoluteUri];
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(s_identityHolderLock);
                }
            }
            return identity;
        }
        [SecurityCritical]
        public static Identity FindOrCreateIdentity(Uri objectUri, Uri uri)
        {
            throw new NotImplementedException();
        }
    }
}
