using System;
using System.Collections.Generic;
using System.Runtime.Loader;
using System.Text;

namespace SubSonic.Core.Remoting.Testing.Components
{
    internal class RemoteAssemblyLoadContext
        : AssemblyLoadContext
    {
        public RemoteAssemblyLoadContext()
            : base(true) { }
    }
}
