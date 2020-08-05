using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;

namespace SubSonic.Core.Remoting.Contracts
{

    /// <summary>
    /// Provides conduits for messages that cross process boundaries.
    /// </summary>
    [ComVisible(true)]
    public interface IChannel
        : IDisposable
    {
        /// <summary>
        /// Gets the priority of the channel.
        /// </summary>
        /// <returns>An integer that indicates the priority of the channel.</returns>
        int ChannelPriority { get; }
        /// <summary>
        /// Gets the name of the channel.
        /// </summary>
        /// <returns>The name of the channel.</returns>
        string ChannelName { get; }

        Task<IChannel> InitializeAsync();
        /// <summary>
        /// Returns the object URI as an out parameter, and the URI of the current channel
        /// as the return value.
        /// </summary>
        /// <param name="url">The URL of the object.</param>
        /// <param name="method">When this method returns, contains a System.String that holds the object URI.
        /// This parameter is passed uninitialized.</param>
        /// <returns></returns>
        [SecurityCritical]
        string Parse(Uri uri, out string method);
    }
    

}
