using SubSonic.Core.Remoting.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;

namespace SubSonic.Core.Remoting.Channels
{
    internal class RegisteredChannelList
        : IEnumerable<RegisteredChannel>
    {
        private readonly RegisteredChannel[] channels;
        public RegisteredChannelList()
        {
            channels = new RegisteredChannel[0];
        }

        public RegisteredChannel this[int i] => channels[i];

        public RegisteredChannelList(IEnumerable<RegisteredChannel> channels)
        {
            this.channels = channels.ToArray();
        }

        public int Count => channels?.Length ?? default;

        public int FindChannelIndex(IChannel channel)
        {
            for(int i = 0, n = channels.Length; i < n; i++)
            {
                if (channel == channels[i])
                {
                    return i;
                }
            }
            return -1;
        }

        [SecurityCritical]
        public int FindChannelIndex(string name)
        {
            for (int i = 0, n = channels.Length; i < n; i++)
            {
                if (string.Compare(name, GetChannel(i).ChannelName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public IChannel GetChannel(int i)
        {
            return this[i].Channel;
        }

        public bool IsReceiver(int index)
        {
            return this[index].IsReceiver;
        }

        public bool IsSender(int index)
        {
            return this[index].IsSender;
        }

        public IEnumerator<RegisteredChannel> GetEnumerator()
        {
            return channels.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return channels.GetEnumerator();
        }

        public int ReceiverCount
        {
            get
            {
                if (this.channels == null)
                {
                    return 0;
                }
                int num = 0;
                for (int i = 0, n = channels.Length; i < n; i++)
                {
                    if (this.IsReceiver(i))
                    {
                        num++;
                    }
                }
                return num;
            }
        }

        public int SenderCount
        {
            get
            {
                if (this.channels == null)
                {
                    return 0;
                }
                int num = 0;
                for (int i = 0, n = channels.Length; i < n; i++)
                {
                    if (this.IsReceiver(i))
                    {
                        num++;
                    }
                }
                return num;
            }
        }
    }
}
