using System;
using System.Collections;
using System.Globalization;

namespace SubSonic.Core.Remoting.Channels.Ipc
{
    public class NamedPipeChannel<TMessageType>
        : IpcChannel
    {
        //private PipeClient<TMessageType> pipeClient;

        public NamedPipeChannel(IDictionary properties)
            : base(properties)
        {
            this.ChannelPriority = 1;
            this.ChannelName = "named pipe";
        }

        public override object ChannelData
        {
            get;
            protected set;
        }

        public bool AutoReconnect
        {
            get;
            protected set;
        }

        public TimeSpan? ReconnectionInterval
        {
            get;
            protected set;
        }

        public Uri GetChannelUri()
        {
            return new Uri($@"ipc:\\{ChannelName}");
        }

        public override IChannel Initialize()
        {
            base.Initialize();

            foreach(DictionaryEntry entry in properties)
            {
                if (entry.Key is string key)
                {
                    if (key == nameof(AutoReconnect))
                    {
                        AutoReconnect = Convert.ToBoolean(entry.Value, CultureInfo.InvariantCulture);
                    }
                    else if (key == nameof(ReconnectionInterval))
                    {
                        if (TimeSpan.TryParse((string)entry.Value, out TimeSpan reconnectionInterval))
                        {
                            ReconnectionInterval = reconnectionInterval;
                        }
                    }
                }
            }

            //pipeClient = new PipeClient<TMessageType>(ChannelName, reconnectionInterval: ReconnectionInterval,  formatter: new H.Formatters.BinaryFormatter())
            //{
            //    AutoReconnect = AutoReconnect
            //};

            return this;
        }

        public override IMessageSink CreateMessageSink(Uri uri, object remoteChannelData, out string objectURI)
        {
            string pipename = null;

            objectURI = null;

            if (uri != null)
            {
                pipename = Parse(uri, out objectURI);
            }
            else if ((remoteChannelData != null) && 
                     (remoteChannelData is IChannelDataStore store))
            {
                if (this.Parse(store.ChannelUris[0], out objectURI) != null)
                {
                    pipename = store.ChannelUris[0].Host;
                }
            }
            if (pipename.IsNullOrEmpty() || uri == null)
            {
                return null;
            }

            return clientChannelSinkProvider.CreateSink(this, uri, remoteChannelData);            
        }

        public override string[] GetUrlsForUri(string objectURI)
        {
            throw new NotImplementedException();
        }

        public override string Parse(Uri uri, out string method)
        {
            method = uri.LocalPath.Substring(1);

            return uri.Host;
        }

        public override void StartListening(object data)
        {
            
        }

        public override void StopListening(object data)
        {
            
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //pipeClient.Dispose();
                //pipeClient = null;
            }

            base.Dispose(disposing);
        }
    }
}
