using StreamingServices.GoodGame.Models;
using System.Runtime.Serialization;

namespace StreamingServices.GoodGame.Chat.Models
{
    [DataContract]
    public class ResGGChatJoin : GGMessage<ResGGChatJoin.DataContainer>
    {
        [DataContract]
        public class DataContainer
        {
            [DataMember(Name = "channel_id")]
            public int ChannelId { get; set; }

            [DataMember(Name = "channel_name")]
            public string ChannelName { get; set; }
        }
    }
}
