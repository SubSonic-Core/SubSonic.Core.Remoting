using System;
using System.Collections;
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
        private static volatile int SetIDCount = 0;
        private static object s_identityHolderLock = new object();
        private static Dictionary<string, WeakReference> URITable = new Dictionary<string, WeakReference>();

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
                    if (URITable[uri.AbsoluteUri].Target is Identity target)
                    {
                        identity = target;
                    }
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
        public static Identity FindOrCreateIdentity(string objectUri, Uri uri, ObjRef objectRef)
        {
            Identity identity = null;

            identity = ResolveIdentity(uri);

            if (identity == null)
            {
                bool lockTaken = false;
                identity = new Identity(objectUri, uri);

                Monitor.Enter(s_identityHolderLock, ref lockTaken);
                try
                {
                    identity = SetIdentity(identity, null);
                    identity.SetObjectRef(objectRef);
                }
                finally
                {
                    if (lockTaken)
                    {
                        Monitor.Exit(s_identityHolderLock);
                    }
                }
            }
            return identity;
        }
        [SecurityCritical]
        private static Identity SetIdentity(Identity identity, object p)
        {
            string key = identity.Uri.AbsoluteUri;

            if (!URITable.ContainsKey(key))
            {
                URITable.Add(key, new WeakReference(identity));
                identity.SetInIdTable();
                SetIDCount++;
                if ((SetIDCount % 0x40) == 0)
                {
                   Task.Run(() => CleanupIdentities());
                }
            }
            else if (URITable[key] is WeakReference reference &&
                     reference.Target is Identity identity1)
            {   // use existing
                identity = identity1;
            }
            else
            {   // renew the table entry
                URITable[key] = new WeakReference(identity);
            }

            return identity;
        }

        private static void CleanupIdentities()
        {
            IDictionaryEnumerator enumerator = URITable.GetEnumerator();
            ArrayList list = new ArrayList();
            while (enumerator.MoveNext())
            {
                if (enumerator.Value is WeakReference reference)
                {
                    if (reference.Target == null)
                    {
                        list.Add(enumerator.Key);
                    }
                }
            }

            foreach (string key in list)
            {
                URITable.Remove(key);
            }
        }
    }
}
