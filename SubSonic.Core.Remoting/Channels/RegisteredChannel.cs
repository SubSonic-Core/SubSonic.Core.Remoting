using SubSonic.Core.Remoting.Contracts;
using System;
using System.Collections.Generic;

namespace SubSonic.Core.Remoting.Channels
{
    internal class RegisteredChannel
        : IEqualityComparer<IChannel>
    {
        private readonly ChannelTypeEnum flags;

        public RegisteredChannel(IChannel channel)
        {
            this.Channel = channel ?? throw new ArgumentNullException(nameof(channel));

            if (channel is IChannelSender)
            {
                flags |= ChannelTypeEnum.Sender;
            }

            if (channel is IChannelReciever)
            {
                flags |= ChannelTypeEnum.Reciever;
            }
        }

        public bool IsReceiver => (flags & ChannelTypeEnum.Reciever) != 0;

        public bool IsSender => (flags & ChannelTypeEnum.Sender) != 0;

        public IChannel Channel { get; private set; }

        public override bool Equals(object obj)
        {
            if (obj is IChannel channel)
            {
                return Equals(Channel, channel);
            }
            return base.Equals(obj);
        }

        public bool Equals(IChannel x, IChannel y)
        {
            return x.Equals(y);
        }

        public override int GetHashCode() => Channel.GetHashCode();

        public int GetHashCode(IChannel obj)
        {
            return obj.GetHashCode();
        }

        public static bool operator ==(IChannel x, RegisteredChannel y)
        {
            return y.Equals(x);
        }

        public static bool operator !=(IChannel x, RegisteredChannel y)
        {
            return !y.Equals(x);
        }
    }
}
