using SubSonic.Core.Remoting.Channels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.Remoting
{
    public static class RemotingServices
    {
        public static async Task<TType> ConnectAsync<TType>(Uri uri)
        {
            if (await ConnectAsync(typeof(TType), uri) is TType success)
            {
                return success;
            }
            return default;
        }
        public static async Task<object> ConnectAsync(Type typeToProxy, Uri uri)
        {
            if (typeToProxy == null)
            {
                throw new ArgumentNullException(nameof(typeToProxy));
            }

            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            return await ChannelServices.ConnectInternalAsync(typeToProxy, uri);
        }
    }
}
