using StreamingServices.GoodGame.Models;
using System.Runtime.Serialization;

namespace StreamingServices.GoodGame.Chat.Models
{
    public class ResGGUnJoin : GGMessage<ResGGUnJoin.DataContainer>
    {
        public class DataContainer
        {
            [DataMember(Name = "channel_id")]
            public int ChannelId { get; set; }
        }
    }
}
