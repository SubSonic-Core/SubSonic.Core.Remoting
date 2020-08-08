using ServiceWire.NamedPipes;
using SubSonic.Core.Remoting.Channels.Services;
using SubSonic.Core.Remoting.Contracts;
using SubSonic.Core.Remoting.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.Remoting.Channels.Ipc.NamedPipes
{
    public class NamedPipeChannel<TService>
        : IpcChannel
        where TService: class, IPipeServices
    {
        private Uri[] supportedUri;

        private NpClient<TService> NpClient = null;

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
            if (supportedUri == null && !IsConnected)
            {
                return Array.Empty<Uri>();
            }

            return supportedUri ?? (supportedUri = NpClient.Proxy.GetAllChannelUri());
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

        public override async Task<object> Invoke(Type typeOfProxy, Uri uri)
        {
            MethodHelper helper = new MethodHelper(uri.LocalPath);

            if (typeof(TService).GetMethod(helper.Name, BindingFlags.Public | BindingFlags.Instance) is MethodInfo method)
            {
                object[] parameters = new object[method.GetParameters().Length];

                for(int i = 0, n = method.GetParameters().Length; i < n; i++)
                {
                    var parameter = method.GetParameters()[i];

                    if (parameter.ParameterType == typeof(Guid))
                    {
                        parameters[i] = new Guid(helper.Parameters.ElementAt(i));
                    }
                    else
                    {
                        parameters[i] = Convert.ChangeType(helper.Parameters.ElementAt(i), parameter.ParameterType);
                    }
                }

                object result =  await Task.Run(() => method.Invoke(NpClient.Proxy, parameters));

                if (typeOfProxy.IsAssignableFrom(result.GetType()))
                {
                    return result;
                }
            }
            return default;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                NpClient?.Dispose();
                NpClient = null;
            }

            
        }
    }
}
