using System;
using System.Collections.Generic;
using System.Linq;
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

        public virtual bool IsRunning { get; protected set; }

        public Uri[] GetAllChannelUri()
        {
            List<Uri> list = new List<Uri>();

            foreach(MethodInfo method in GetType().GetMethods())
            {
                if (method.Name == nameof(GetAllChannelUri))
                {
                    continue;
                }

                IEnumerable<string> parameters = method.GetParameters().Select(x => $"{{{x.Name}}}");

                list.Add(new Uri(serviceUri, $"{method.Name}{(parameters.Count() == 0 ? "" : $"/{parameters.Join("/")}")}"));
            }

            return list.ToArray();
        }
    }
}
