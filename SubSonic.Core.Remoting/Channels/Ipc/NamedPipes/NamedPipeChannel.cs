using ServiceWire.NamedPipes;
using SubSonic.Core.Remoting.Channels.Services;
using SubSonic.Core.Remoting.Contracts;
using SubSonic.Core.Remoting.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.Remoting.Channels.Ipc.NamedPipes
{
    public class NamedPipeChannel<TService>
        : IpcChannel
        where TService: class, IPipeServices
    {
        NpClient<TService> NpClient = null;

        public NamedPipeChannel()
            : this(new Hashtable()) { }

        public NamedPipeChannel(IDictionary properties, ISerializationProvider serialization = null)
            : base(properties, serialization)
        {
            ChannelPriority = 1;
            EnableAutomaticConnection = true;
            ReconnectionInterval = 1000;
            ChannelName = typeof(TService).Name;
        }

        public bool EnableAutomaticConnection { get; protected set; }

        public int ReconnectionInterval { get; protected set; }

        public override Uri[] GetAllChannelUri()
        {
            if (!IsConnected)
            {
                return Array.Empty<Uri>();
            }

            return NpClient.Proxy.GetAllChannelUri();
        }

        public override bool IsConnected => NpClient?.IsConnected ?? default;

        public override IChannel Initialize()
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
                    else if (key == nameof(EnableAutomaticConnection))
                    {
                        EnableAutomaticConnection = Utilities.Cast<bool>(entry.Value);
                    }
                    else if (key == nameof(ReconnectionInterval))
                    {
                        ReconnectionInterval = Utilities.Cast<int>(entry.Value);
                    }
                }
            }

            return this;
        }

        public override async Task ConnectAsync()
        {
            while(!IsConnected)
            {
                try
                {
                    NpClient = new NpClient<TService>(new NpEndPoint(ChannelName), serialization);
                }
                catch (TimeoutException)
                {
                    if(!EnableAutomaticConnection)
                    {
                        throw;
                    }

                    await Task.Delay(ReconnectionInterval);
                }
            }
        }

        public override async Task<object> Invoke(Uri uri)
        {
            if (typeof(TService).GetMethod(uri.LocalPath.Substring(1), BindingFlags.Public | BindingFlags.Instance) is MethodInfo method)
            {
                return await Task.Run(() => method.Invoke(NpClient.Proxy, new object[] { Guid.NewGuid() }));
            }
            return default;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                NpClient?.Dispose();
                NpClient = null;
            }

            Dispose(disposing);
        }
    }
}
