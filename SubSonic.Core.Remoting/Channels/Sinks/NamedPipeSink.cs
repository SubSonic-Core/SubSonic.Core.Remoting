using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.Remoting.Channels.Sinks
{
    public class NamedPipeSink
        : IClientFormatterSink, IMessageSink, IClientChannelSink, IChannelSinkBase
    {
        public NamedPipeSink (IDictionary properties, IClientChannelSink nextChannelSink)
        {
            Properties = properties ?? throw new ArgumentNullException(nameof(properties));
            NextChannelSink = nextChannelSink;
        }

        public SinkChannelProtocolEnum ChannelProtocol { get; set; }

        public IDictionary Properties { get; }

        public IClientChannelSink NextChannelSink { get; }

        public IMessageSink NextSink => null;

        public bool IncludeVersioning { get; set; }

        public bool StrictBinding { get; set; }

        public IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink)
        {
            throw new NotImplementedException();
        }

        public Stream GetRequestStream(IMessage msg, ITransportHeaders headers)
        {
            throw new NotImplementedException();
        }

        public void ProcessMessage(IMessage msg, ITransportHeaders requestHeaders, Stream requestStream, out ITransportHeaders responseHeaders, out Stream responseStream)
        {
            throw new NotImplementedException();
        }

        public Task ProcessRequestAsync(IClientChannelSinkStack sinkStack, IMessage msg, ITransportHeaders headers, Stream stream)
        {
            throw new NotImplementedException();
        }

        public Task ProcessResponseAsync(IClientResponseChannelSinkStack sinkStack, object state, ITransportHeaders headers, Stream stream)
        {
            throw new NotImplementedException();
        }

        public IMessage SyncProcessMessage(IMessage msg)
        {
            throw new NotImplementedException();
        }
    }
}
