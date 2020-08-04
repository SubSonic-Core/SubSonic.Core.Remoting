using SubSonic.Core.Remoting.Channels.Sinks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Text;

namespace SubSonic.Core.Remoting.Channels.Providers
{
    public class NamedPipeSinkProvider
        : IClientChannelSinkProvider
    {
        private readonly IDictionary properties;
        private IClientChannelSinkProvider next;
        private bool includeVersioning;
        private bool strictBinding;

        public NamedPipeSinkProvider()
        {
            includeVersioning = true;
        }

        public NamedPipeSinkProvider(IDictionary properties)
            : this()
        {
            this.properties = properties;
        }

        public IClientChannelSinkProvider Next
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure, Infrastructure = true)]
            get
            {
                return this.next;
            }
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure, Infrastructure = true)]
            set
            {
                this.next = value;
            }
        }

        public IClientChannelSink CreateSink(IChannelSender channel, Uri uri, object remoteChannelData)
        {
            IClientChannelSink nextSink = null;

            if (Next != null)
            {
                nextSink = Next.CreateSink(channel, uri, remoteChannelData);
                if (nextSink == null)
                {
                    return null;
                }
            }

            return new NamedPipeSink(channel.Properties, nextSink)
            {
                IncludeVersioning = this.includeVersioning,
                StrictBinding = this.strictBinding,
                ChannelProtocol = uri.Scheme.Parse<SinkChannelProtocolEnum>()
            };
        }
    }
}
