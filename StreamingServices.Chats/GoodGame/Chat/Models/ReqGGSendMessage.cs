using Newtonsoft.Json;

namespace StreamingServices.Chats.GoodGame.Chat.Models
{
    internal class ReqGGSendMessage
    {
        [JsonProperty(PropertyName = "channel_id")]
        public int ChannelId { get; set; }

        [JsonProperty(PropertyName = "color")]
        public string Color { get; set; }

        [JsonProperty(PropertyName = "icon")]
        public string Icon { get; set; }

        [JsonProperty(PropertyName = "mobile")]
        public int Mobile { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }
    }
}
