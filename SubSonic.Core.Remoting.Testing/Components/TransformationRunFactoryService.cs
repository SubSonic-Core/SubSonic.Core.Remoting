﻿using Mono.TextTemplating;
using Mono.VisualStudio.TextTemplating;
using Mono.VisualStudio.TextTemplating.VSHost;
using NSubstitute;
using NUnit.Framework.Constraints;
using SubSonic.Core.Remoting.Channels.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Factory = Mono.VisualStudio.TextTemplating.VSHost.TransformationRunFactory;

namespace SubSonic.Core.Remoting.Testing.Components
{
    [Serializable]
    public class TransformationRunFactoryService
        : BasePipeService
        , ITransformationRunFactoryService
        , IDisposable
    {
        private bool disposedValue;

        internal readonly static ConcurrentDictionary<Guid, IProcessTransformationRunFactory> runFactories = new ConcurrentDictionary<Guid, IProcessTransformationRunFactory>();

        public TransformationRunFactoryService(Uri serviceUri)
            : base(serviceUri) { }

        public IProcessTransformationRunFactory TransformationRunFactory(Guid id)
        {
            IProcessTransformationRunFactory factory = new Factory(id)
            {
                IsAlive = true
            };

            if (runFactories.TryAdd(id, factory))
            {
                return factory;
            }

            return default;
        }

        public bool Shutdown(Guid id)
        {
            if (runFactories.TryGetValue(id, out IProcessTransformationRunFactory factory))
            {
                foreach (var entry in Factory.Runners)
                {
                    if (entry.Value is TransformationRunner runner)
                    {
                        if (id == runner.Factory.ID)
                        {
                            factory.DisposeOfRunner(runner.RunnerId);
                        }
                    }
                }
            }

            return IsRunning = Factory.Runners.Count > 0;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var entry in runFactories)
                    {
                        if (entry.Value is Factory factory)
                        {
                            Shutdown(factory.ID);

                            runFactories.TryRemove(factory.ID, out _);
                        }
                    }
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
