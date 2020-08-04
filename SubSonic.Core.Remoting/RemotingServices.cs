using SubSonic.Core.Remoting.Channels;
using SubSonic.Core.VisualStudio.Common;
using SubSonic.Core.VisualStudio.Common.SubSonic.Core.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.Remoting
{
    public static class RemotingServices
    {
        public static async Task<object> ConnectAsync<TType>(Uri uri)
        {
            return await ConnectAsync(typeof(TType), uri);
        }

        public static async Task<object> ConnectAsync(Type classToProxy, Uri uri)
        {
            return await UnmarshalAsync(classToProxy, uri);
        }

        internal static async Task<object> UnmarshalAsync(Type classToProxy, Uri uri, object data = null)
        {
            if (classToProxy is null)
            {
                throw new ArgumentNullException(nameof(classToProxy));
            }

            if (uri is null)
            {
                throw new ArgumentNullException(nameof(uri));
            }
            if (!classToProxy.IsMarshalByRef && !classToProxy.IsInterface)
            {
                throw new SubSonicRemotingException(RemotingResources.NotRemotableByReference.Format(classToProxy));
            }

            Identity identity = IdentityHolder.ResolveIdentity(uri);

            if (identity == null)
            {
                Uri objectUri = CreateChannelSink(uri, data, out IMessageSink messageSink);
                if (messageSink == null)
                {
                    throw new SubSonicRemotingException(RemotingResources.CannotCreateChannelSink.Format(uri));
                }
                if (objectUri == null)
                {
                    throw new ArgumentException(RemotingResources.InvalidUrl.Format(uri), nameof(uri));
                }
                identity = IdentityHolder.FindOrCreateIdentity(objectUri, uri);

                SetChannelSink(identity, messageSink);
            }

            return GetOrCreateProxy(classToProxy, identity);
        }

        private static object GetOrCreateProxy(Type classToProxy, Identity identity)
        {
            return null;
        }

        private static void SetChannelSink(Identity identity, IMessageSink channelSink)
        {
            if (identity != null)
            {
                if (channelSink != null)
                {
                    identity.SetChannelSink(channelSink);
                }
            }
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.RemotingConfiguration)]
        public static void SetObjectUriForMarshal(MarshalByRefObject obj, Uri uri)
        {
            throw new NotImplementedException();
        }

        [SecurityCritical]
        private static Uri CreateChannelSink(Uri uri, object data, out IMessageSink messageSink)
        {
            Uri objectURI = null;
            messageSink = ChannelServices.CreateMessageSink(uri, data, out objectURI);
            //if (messageSink == null)
            //{
            //    object obj2 = s_delayLoadChannelLock;
            //    lock (obj2)
            //    {
            //        messageSink = ChannelServices.CreateMessageSink(url, data, out objectURI);
            //        if (messageSink == null)
            //        {
            //            messageSink = RemotingConfigHandler.FindDelayLoadChannelForCreateMessageSink(url, data, out objectURI);
            //        }
            //    }
            //}
            return objectURI;
        }
    }
}
