using Mono.TextTemplating;
using Mono.VisualStudio.TextTemplating;
using Mono.VisualStudio.TextTemplating.VSHost;
using NSubstitute;
using NUnit.Framework.Constraints;
using SubSonic.Core.Remoting.Channels.Services;
using SubSonic.Core.Testing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.VisualStudio.Testing.Components
{
    [Serializable]
    public class TransformationRunFactoryService
        : BasePipeService
        , ITransformationRunFactoryService
        , IDisposable
    {

        public TransformationRunFactoryService(Uri serviceUri)
            : base(serviceUri)
        {

        }

        private bool disposedValue;

        internal readonly static ConcurrentDictionary<Guid, IProcessTransformationRunFactory> runFactories = new ConcurrentDictionary<Guid, IProcessTransformationRunFactory>();

        public IProcessTransformationRunFactory TransformationRunFactory(Guid id)
        {
            IProcessTransformationRunFactory factory = new RemoteTransformationRunFactory(id)
            {
                IsAlive = true
            };

            if (runFactories.TryAdd(id, factory))
            {
                return factory;
            }

            return default;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    runFactories.Clear();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TransformationRunFactoryService()
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
