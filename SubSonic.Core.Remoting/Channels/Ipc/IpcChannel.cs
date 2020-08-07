using SubSonic.Core.Remoting.Contracts;
using SubSonic.Core.Remoting.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SubSonic.Core.Remoting.Channels.Ipc
{
    public abstract class IpcChannel
        : IChannel, IChannelReciever, IChannelSender
    {
        protected readonly ISerializationProvider serialization;
        private bool disposedValue;

        public IpcChannel(IDictionary properties, ISerializationProvider serialization = null)
        {
            Properties = properties ?? throw new ArgumentNullException(nameof(properties));
            this.serialization = serialization ?? new BinarySerializationProvider();
        }

        public IDictionary Properties { get; }

        public int ChannelPriority { get; protected set; }

        public string ChannelName { get; protected set; }

        public abstract bool IsConnected { get; }

        public virtual Uri ChannelUri => new Uri($"ipc://{ChannelName}");

        public abstract Uri[] GetAllChannelUri();

        public abstract IChannel Initialize();

        public string Parse(Uri uri, out string method)
        {
            throw new NotImplementedException();
        }

        public abstract Task ConnectAsync();

        public virtual async Task<bool> IsUriSupportedAsync(Uri uri)
        {
            await ConnectAsync().ConfigureAwait(false);

            foreach(Uri serviceUri in GetAllChannelUri())
            {
                MethodHelper
                    serviceMethod = new MethodHelper(serviceUri.LocalPath),
                    uriMethod = new MethodHelper(uri.LocalPath);

                if (serviceUri.Scheme == uri.Scheme &&
                    serviceUri.Host == uri.Host &&
                    serviceMethod == uriMethod)
                {
                    return true;
                }
            }
            return default;
        }

        public abstract Task<object> Invoke(Type typeOfProxy, Uri uri);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~IpcChannel()
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
