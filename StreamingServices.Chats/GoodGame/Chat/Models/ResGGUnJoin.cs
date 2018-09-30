using Newtonsoft.Json;

namespace StreamingServices.GoodGame.Chat.Models
{
    internal class GGUnJoin
    {
        [JsonProperty(PropertyName = "channel_id")]
        public int ChannelId { get; set; }
    }
}
