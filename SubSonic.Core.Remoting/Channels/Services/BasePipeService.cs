using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SubSonic.Core.Remoting.Channels.Services
{
    public abstract class BasePipeService
        : IPipeServices
    {
        private readonly Uri serviceUri;

        protected BasePipeService(Uri serviceUri)
        {
            this.serviceUri = serviceUri ?? throw new ArgumentNullException(nameof(serviceUri));

            IsRunning = true;
        }

        public bool IsRunning { get; protected set; }

        public Uri[] GetAllChannelUri()
        {
            List<Uri> list = new List<Uri>();

            foreach(MethodInfo method in GetType().GetMethods())
            {
                if (method.Name == nameof(GetAllChannelUri))
                {
                    continue;
                }

                list.Add(new Uri(serviceUri, method.Name));
            }

            return list.ToArray();
        }

        public virtual bool Shutdown()
        {
            return (IsRunning = false);
        }
    }
}
