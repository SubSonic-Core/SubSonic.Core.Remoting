using Mono.VisualStudio.TextTemplating;
using Mono.VisualStudio.TextTemplating.VSHost;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting.Testing.Components
{
    [Serializable]
    public class RemoteTransformationRunFactory
        : TransformationRunFactory
    {
        public RemoteTransformationRunFactory(Guid id)
            : base(id) { }

        public override IProcessTransformationRunner CreateTransformationRunner()
        {
            Guid runnerId = Guid.NewGuid();

            IProcessTransformationRunner runner = new RemoteTransformationRunner(this, runnerId);

            if (Runners.TryAdd(runnerId, runner))
            {
                return runner;
            }

            return default;
        }
    }
}
