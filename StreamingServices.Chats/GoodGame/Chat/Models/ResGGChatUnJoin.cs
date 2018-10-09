using Newtonsoft.Json;

namespace StreamingServices.GoodGame.Chat.Models
{
    internal class ResGGChatUnJoin
    {
        [JsonProperty(PropertyName = "channel_id")]
        public int ChannelId { get; set; }
    }
}
