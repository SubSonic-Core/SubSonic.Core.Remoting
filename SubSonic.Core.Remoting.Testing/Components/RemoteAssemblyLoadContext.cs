using System;
using System.Collections.Generic;
using System.Runtime.Loader;
using System.Text;

namespace SubSonic.Core.VisualStudio.Testing.Components
{
    internal class RemoteAssemblyLoadContext
        : AssemblyLoadContext
    {
        public RemoteAssemblyLoadContext()
            : base(true) { }
    }
}
