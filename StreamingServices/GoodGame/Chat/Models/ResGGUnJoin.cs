using Newtonsoft.Json;
using StreamingServices.GoodGame.Models;

namespace StreamingServices.GoodGame.Chat.Models
{
    public class ResGGUnJoin : GGMessage<ResGGUnJoin.DataContainer>
    {
        public class DataContainer
        {
            [JsonProperty(PropertyName = "channel_id")]
            public int ChannelId { get; set; }
        }
    }
}
