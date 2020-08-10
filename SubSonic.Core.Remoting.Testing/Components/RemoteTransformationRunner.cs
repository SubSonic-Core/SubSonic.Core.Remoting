using Mono.VisualStudio.TextTemplating.VSHost;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace SubSonic.Core.Remoting.Testing.Components
{
    [Serializable]
    public class RemoteTransformationRunner
        : TransformationRunner
    {
        public RemoteTransformationRunner(TransformationRunFactory factory, Guid runnerId)
            : base(factory, runnerId) { }

        public override Assembly LoadFromAssemblyName(AssemblyName assemblyName)
        {
            return Assembly.Load(assemblyName);
        }

        protected override void Unload()
        {
            throw new NotSupportedException();
        }
    }
}
