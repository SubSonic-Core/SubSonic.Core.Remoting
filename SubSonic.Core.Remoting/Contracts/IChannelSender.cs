using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.Remoting.Contracts
{
    public interface IChannelSender
    {
        Uri ChannelUri { get; }

        Task<bool> IsUriSupportedAsync(Uri uri);

        Task<object> Invoke(Type typeOfProxy, Uri uri);
    }
}
