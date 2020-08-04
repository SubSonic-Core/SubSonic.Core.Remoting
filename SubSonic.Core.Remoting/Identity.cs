using SubSonic.Core.VisualStudio.Common;
using SubSonic.Core.VisualStudio.Common.SubSonic.Core.Remoting;
using System;
using System.Threading;

namespace SubSonic.Core.Remoting
{
    public class Identity
    {
        private readonly string url;
        protected IdentityEnum flags;
        private Uri ObjURI;

        private IMessageSink channelSink;

        public Identity(bool bContextBound)
        {
            if (bContextBound)
            {
                flags |= IdentityEnum.CONTEXT_BOUND;
            }
        }

        public Identity(Uri objURI, string url)
        {
            if (url != null)
            {
                this.flags |= IdentityEnum.WELLKNOWN;
                this.url = url;
            }
            this.SetOrCreateURI(objURI, true);
        }

        public IMessageSink ChannelSink => this.channelSink;

        public void SetOrCreateURI(Uri objURI, bool bIdCtor)
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
    }
}
