using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Core.Remoting.Channels.Services
{
    public interface IPipeServices
    {
        /// <summary>
		/// get a list of unique resource identifiers supported by this service
		/// </summary>
		/// <returns></returns>
		Uri[] GetAllChannelUri();
	}
}
