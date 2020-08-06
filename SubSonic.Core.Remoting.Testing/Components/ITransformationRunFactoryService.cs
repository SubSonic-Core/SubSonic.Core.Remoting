using Mono.VisualStudio.TextTemplating;
using SubSonic.Core.Remoting.Channels.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Testing
{
    public interface ITransformationRunFactoryService
        : IPipeServices
    {
        /// <summary>
		/// Starts up a transformation run factory
		/// </summary>
		/// <returns>rpc reference to a transformation run factory</returns>
		IProcessTransformationRunFactory TransformationRunFactory(Guid id);
    }
}
