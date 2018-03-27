using Newtonsoft.Json;
using StreamingServices.GoodGame.Models;

namespace StreamingServices.GoodGame.Chat.Models
{
    public class ResGGSendMessage : GGMessage<ResGGSendMessage.DataContainer>
    {
        public class DataContainer
        {
            [JsonProperty(PropertyName = "channel_id")]
            public int ChannelId { get; set; }

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

            [JsonProperty(PropertyName = "hideIcon")]
            public bool HideIcon { get; set; }

            [JsonProperty(PropertyName = "mobile")]
            public bool Mobile { get; set; }

            [JsonProperty(PropertyName = "color")]
            public string Color { get; set; }
        }

        public override string ToString()
        {
            return $"{Data.UserName}: {Data.Text}";
        }
    }
}
