using Mono.TextTemplating;
using Mono.VisualStudio.TextTemplating;
using Mono.VisualStudio.TextTemplating.VSHost;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.VisualStudio.Testing.Components
{
    [Serializable]
    public class RemoteTransformationRunFactory
        : TransformationRunFactory
    {
        public RemoteTransformationRunFactory(Guid id)
            : base(id)
        {
        }

        public override string StartTransformation(IProcessTransformationRunner runner)
        {
            throw new NotImplementedException();
        }
    }
}
