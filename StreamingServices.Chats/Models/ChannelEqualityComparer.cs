using System;
using System.Collections.Generic;

namespace StreamingServices.Chats.Models
{
    class ChannelEqualityComparer : IEqualityComparer<Channel>
    {
        public bool Equals(Channel channel1, Channel channel2)
        {
            if (channel1 == null || channel2 == null)
                throw new ArgumentNullException();

            return channel1.Equals(channel2);
        }

        public int GetHashCode(Channel channel)
        {
            if (channel == null)
                throw new ArgumentNullException(nameof(channel));

            return channel.GetHashCode();
        }
    }
}
