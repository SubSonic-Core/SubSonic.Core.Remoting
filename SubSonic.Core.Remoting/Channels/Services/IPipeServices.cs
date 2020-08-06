using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting.Channels.Services
{
    public interface IPipeServices
    {
        /// <summary>
		/// get a string array of methods contained in this service
		/// </summary>
		/// <returns></returns>
		string[] GetChannelSupportedUri();

		/// <summary>
		/// implements the ability to shutdown a local hosted service instance
		/// </summary>
		/// <returns></returns>
		bool Shutdown();
	}
}
