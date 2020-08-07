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
		/// <param name="id">id assigned to the run factory instance</param>
		/// <returns>rpc reference to a transformation run factory</returns>
		IProcessTransformationRunFactory TransformationRunFactory(Guid id);

		/// <summary>
		/// implements the ability to shutdown a local hosted service instance
		/// </summary>
		/// <param name="id">id assigned to the run factory instance</param>
		/// <returns>true, if successfull in shutting down the run factory.</returns>
		bool Shutdown(Guid id);
	}
}
