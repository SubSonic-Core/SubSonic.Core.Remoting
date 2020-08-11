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
            : base(factory, runnerId) {
            RemoteTransformationRunFactory.Context.Resolving += ResolveReferencedAssemblies;
        }

        public override Assembly LoadFromAssemblyName(AssemblyName assemblyName)
        {
            return RemoteTransformationRunFactory.Context.LoadFromAssemblyName(assemblyName);
        }

        protected override void Unload()
        {
            RemoteTransformationRunFactory.Context.Resolving -= ResolveReferencedAssemblies;

            RemoteTransformationRunFactory.Context.Unload();
        }
    }
}
