using Mono.TextTemplating;
using Mono.VisualStudio.TextTemplating;
using Mono.VisualStudio.TextTemplating.VSHost;
using NSubstitute;
using NUnit.Framework.Constraints;
using SubSonic.Core.Remoting.Channels.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using RunFactory = Mono.VisualStudio.TextTemplating.VSHost.TransformationRunFactory;

namespace SubSonic.Core.Remoting.Testing.Components
{
    [Serializable]
    public class TransformationRunFactoryService
        : BasePipeService
        , ITransformationRunFactoryService
        , IDisposable
    {
        private bool disposedValue;

        [NonSerialized]
        private IProcessTransformationRunFactory runFactory;

        public TransformationRunFactoryService(Uri serviceUri)
            : base(serviceUri) {
            runFactory ??= new RemoteTransformationRunFactory(Guid.NewGuid());
        }

        public Guid GetFactoryId()
        {
            return runFactory.GetFactoryId();
        }

        public bool IsRunFactoryAlive()
        {
            return runFactory.IsRunFactoryAlive();
        }

        public IProcessTransformationRunFactory TransformationRunFactory()
        {
            throw new InvalidOperationException(RemotingResources.MethodIsStubbedOutForProxyImpersonation);
        }

        public IProcessTransformationRunner CreateTransformationRunner()
        {
            return runFactory.CreateTransformationRunner();
        }

        public bool DisposeOfRunner(Guid runnerId)
        {
            return runFactory.DisposeOfRunner(runnerId); ;
        }

        public bool PrepareTransformation(Guid runnerId, ParsedTemplate pt, string content, ITextTemplatingEngineHost host, TemplateSettings settings)
        {
            return runFactory.PrepareTransformation(runnerId, pt, content, host, settings);
        }

        public string StartTransformation(Guid runnerId)
        {
            return runFactory.StartTransformation(runnerId);
        }

        public bool Shutdown(Guid id)
        {
            foreach (var entry in RunFactory.Runners)
            {
                if (entry.Value is TransformationRunner runner)
                {
                    if (id == runner.Factory.ID)
                    {
                        runFactory.DisposeOfRunner(runner.RunnerId);
                    }
                }
            }

            return IsRunning = RunFactory.Runners.Count > 0;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Shutdown(GetFactoryId());
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
