using SubSonic.Core.VisualStudio.Common;
using SubSonic.Core.VisualStudio.Common.SubSonic.Core.Remoting;
using System;
using System.Threading;

namespace SubSonic.Core.Remoting
{
    public class Identity
    {
        private readonly Uri uri;
        protected IdentityEnum flags;
        private string ObjURI;
        protected object tpOrObject;
        protected object objectRef;

        private IMessageSink channelSink;

        public Identity(bool bContextBound)
        {
            if (bContextBound)
            {
                flags |= IdentityEnum.CONTEXT_BOUND;
            }
        }

        public Identity(string objURI, Uri uri)
        {
            if (uri != null)
            {
                this.flags |= IdentityEnum.WELLKNOWN;
                this.uri = uri;
            }
            this.SetOrCreateURI(objURI, true);
        }

        public IMessageSink ChannelSink => this.channelSink;

        public Uri Uri => uri;

        public MarshalByRefObject ByRefObject => tpOrObject as MarshalByRefObject;

        public ObjRef ObjectRef => objectRef as ObjRef;

        public void SetOrCreateURI(string objURI, bool bIdCtor)
        {
            if (!bIdCtor && (this.ObjURI != null))
            {
                throw new SubSonicRemotingException(RemotingResources.SetObjectUriForMarshal__UriExists.Format(objURI.ToString()));
            }

            this.ObjURI = objURI ?? throw new ArgumentNullException(nameof(objURI));
        }

        public IMessageSink SetChannelSink(IMessageSink channelSink)
        {
            if (this.channelSink == null)
            {
                Interlocked.CompareExchange(ref this.channelSink, channelSink, null);
            }
            return this.channelSink;
        }

        public ObjRef SetObjectRef(ObjRef objectRef)
        {
            if (this.objectRef == null)
            {
                Interlocked.CompareExchange(ref this.objectRef, objectRef, null);
            }
            return this.objectRef as ObjRef;
        }

        public void SetInIdTable()
        {
            flags |= IdentityEnum.IN_IDTABLE;
        }

        
    }
}
