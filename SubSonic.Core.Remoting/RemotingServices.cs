using SubSonic.Core.Remoting.Channels;
using SubSonic.Core.Remoting.Proxies;
using SubSonic.Core.VisualStudio.Common;
using SubSonic.Core.VisualStudio.Common.SubSonic.Core.Remoting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
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
                string objectUri = CreateChannelSink(uri, data, out IMessageSink messageSink);
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

        public static object CreateTransparentProxy(RealProxy realProxy, Type classToProxy, IntPtr stub, object stubData)
        {
            throw new NotImplementedException();
        }

        private static object GetOrCreateProxy(Type classToProxy, Identity identity)
        {
            object byRefObject = identity.ByRefObject ?? SetOrCreateProxy(identity, classToProxy);
            
            if (identity is ServerIdentity serverIdentity)
            {
                if (!classToProxy.IsAssignableFrom(serverIdentity.ServerType))
                {
                    throw new InvalidCastException(RemotingResources.InvalidCast.Format(CultureInfo.CurrentCulture, serverIdentity.ServerType, classToProxy));
                }
            }

            return byRefObject;
        }

        private static MarshalByRefObject SetOrCreateProxy(Identity identity, Type classToProxy, object proxy)
        {
            RealProxy realProxy 
            throw new NotImplementedException();
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
        private static string CreateChannelSink(Uri uri, object data, out IMessageSink messageSink)
        {
            messageSink = ChannelServices.CreateMessageSink(uri, data, out string objectURI);
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

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static extern bool IsTransparentProxy(object proxy);
    }
}
