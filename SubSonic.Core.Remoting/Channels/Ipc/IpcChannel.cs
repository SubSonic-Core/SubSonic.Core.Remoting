using SubSonic.Core.VisualStudio.Common.SubSonic.Core.Remoting.Channels.Ipc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Text;

namespace SubSonic.Core.Remoting.Channels.Ipc
{
    public abstract class IpcChannel
        : IChannelReceiver, IChannel, IChannelSender, IDisposable
    {
        protected readonly IDictionary properties;
        protected IClientChannelSinkProvider clientChannelSinkProvider;

        private bool disposedValue;

        public IpcChannel(IDictionary properties)
        {
            this.properties = properties ?? throw new ArgumentNullException(nameof(properties));

            ChannelPriority = 20;
            ChannelName = "ipc";
        }

        public virtual object ChannelData
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure, Infrastructure = true)]
            get;
            protected set;
        }

        public int ChannelPriority {
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure, Infrastructure = true)]
            get; 
            protected set; 
        }

        public string ChannelName {
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure, Infrastructure = true)]
            get; 
            protected set; 
        }

        public virtual IChannel Initialize()
        {
            foreach (DictionaryEntry entry in properties)
            {
                if (entry.Key is string key)
                {
                    if (key == nameof(ChannelName))
                    {
                        ChannelName = (string)entry.Value;
                    }
                    if (key == nameof(ChannelPriority))
                    {
                        ChannelPriority = Convert.ToInt32(entry.Value, CultureInfo.InvariantCulture);
                    }
                }
            }

            return this;
        }

        public abstract IMessageSink CreateMessageSink(Uri uri, object remoteChannelData, out string objectURI);

        public virtual string[] GetUrlsForUri(string objectURI)
        {
            throw new NotImplementedException();
        }

        public abstract string Parse(Uri uri, out string method);

        public abstract void StartListening(object data);

        public abstract void StopListening(object data);

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
