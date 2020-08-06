using ServiceWire.NamedPipes;
using SubSonic.Core.Remoting.Contracts;
using SubSonic.Core.Remoting.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.Remoting.Channels.Ipc.NamedPipes
{
    public class NamedPipeChannel<TService>
        : IChannel, IChannelReciever, IChannelSender
        where TService: class
    {
        private readonly ISerializationProvider serialization;
        NpClient<TService> NpClient = null;
        private bool disposedValue;

        public NamedPipeChannel()
            : this(new Hashtable())
        {
            ChannelPriority = 1;
            ChannelName = typeof(TService).Name;
        }

        public NamedPipeChannel(IDictionary properties, ISerializationProvider serialization = null)
        {
            Properties = properties ?? throw new ArgumentNullException(nameof(properties));
            this.serialization = serialization ?? new BinarySerializationProvider();
        }

        public int ChannelPriority { get; protected set; }

        public string ChannelName { get; protected set; }

        public bool IsConnected => NpClient?.IsConnected ?? default;

        public IDictionary Properties { get; }

        public async Task<IChannel> InitializeAsync()
        {
            foreach (DictionaryEntry entry in Properties)
            {
                if (entry.Key is string key)
                {
                    if (key == nameof(ChannelName))
                    {
                        ChannelName = Utilities.Cast<string>(entry.Value);
                    }
                    else if (key == nameof(ChannelPriority))
                    {
                        ChannelPriority = Utilities.Cast<int>(entry.Value);
                    }
                }
            }

            NpClient = new NpClient<TService>(new NpEndPoint(ChannelName), serialization);

            return this;
        }

        public Uri ChannelUri => new Uri($"ipc://{ChannelName}");

        public string Parse(Uri uri, out string method)
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    NpClient?.Dispose();
                    NpClient = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~NamedPipeChannel()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
