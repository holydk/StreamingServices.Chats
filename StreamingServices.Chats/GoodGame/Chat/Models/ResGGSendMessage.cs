using Newtonsoft.Json;

namespace StreamingServices.GoodGame.Chat.Models
{
    internal class ResGGSendMessage
    {
        [JsonProperty(PropertyName = "channel_id")]
        public int ChannelId { get; set; }

        [JsonProperty(PropertyName = "color")]
        public string Color { get; set; }

        [JsonProperty(PropertyName = "user_id")]
        public int UserId { get; set; }

        [JsonProperty(PropertyName = "user_name")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "message_id")]
        public decimal MessageId { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public int Timestamp { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "icon")]
        public string Icon { get; set; }

        [JsonProperty(PropertyName = "mobile")]
        public bool Mobile { get; set; }

        [JsonProperty(PropertyName = "user_rights")]
        public int UserRights { get; set; }

        public override string ToString()
        {
            return $"{UserName}: {Text}";
        }
    }
}
