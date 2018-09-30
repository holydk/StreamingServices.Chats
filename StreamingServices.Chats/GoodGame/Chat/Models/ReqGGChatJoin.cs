using Newtonsoft.Json;

namespace StreamingServices.Chats.GoodGame.Chat.Models
{
    internal class ReqGGChatJoin
    {
        [JsonProperty(PropertyName = "channel_id")]
        public int ChannelId { get; set; }

        [JsonProperty(PropertyName = "hidden")]
        public int Hidden { get; set; }
    }
}
