using SubSonic.Core.Remoting.Contracts;
using System;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;

namespace SubSonic.Core.Remoting.Channels
{
    public static class ChannelServices
    {
        private static readonly object s_channelLock = new object();
        private static volatile RegisteredChannelList s_registeredChannels = new RegisteredChannelList();

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.RemotingConfiguration)]
        public static async Task<bool> RegisterChannelAsync(IChannel channel, bool ensureSecurity = false)
        {
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }

            await channel.InitializeAsync();

            bool
                lockTaken = false,
                success = false;
            try
            {
                Monitor.Enter(s_channelLock, ref lockTaken);

                RegisteredChannelList list = s_registeredChannels;

                if (channel.ChannelName.IsNotNullOrEmpty() && list.FindChannelIndex(channel.ChannelName) > -1)
                {
                    throw new SubSonicRemotingException(RemotingResources.ChannelNameAlreadyRegistered.Format(channel.ChannelName));
                }
                if (ensureSecurity)
                {
                    if (channel is ISecurableChannel securable)
                    {
                        securable.IsSecured = true;
                    }
                    else
                    {
                        throw new SubSonicRemotingException(RemotingResources.CannotBeSecured.Format(channel.ChannelName));
                    }
                }

                RegisteredChannel[] channels = new RegisteredChannel[list.Count + 1];

                int
                    priority = channel.ChannelPriority,
                    index = 0;

                while (true)
                {
                    if (index < list.Count)
                    {
                        RegisteredChannel channel1 = list[index];
                        if (priority <= channel1.Channel.ChannelPriority)
                        {
                            channels[index] = channel1;
                            index++;
                            continue;
                        }
                        channels[index] = new RegisteredChannel(channel);
                    }
                    if (index == list.Count)
                    {
                        channels[index] = new RegisteredChannel(channel);
                    }
                    else
                    {
                        while (index < list.Count)
                        {
                            channels[index + 1] = list[index];
                            index++;
                        }
                    }
                    s_registeredChannels = new RegisteredChannelList(channels);
                    break;
                }

                success = true;
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(s_channelLock);
                }
            }

            return success;
        }
    }
}
