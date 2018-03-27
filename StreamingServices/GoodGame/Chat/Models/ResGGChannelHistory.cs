using Newtonsoft.Json;
using StreamingServices.GoodGame.Models;

namespace StreamingServices.GoodGame.Chat.Models
{
    public class ResGGChannelHistory : GGMessage<ResGGChannelHistory.DataContainer>
    {
        public class DataContainer
        {
            [JsonProperty(PropertyName = "channel_id")]
            public int ChannelId { get; set; }

            [JsonProperty(PropertyName = "messages")]
            public Message[] Messages { get; set; }

            public class Message
            {
                [JsonProperty(PropertyName = "user_id")]
                public int UserId { get; set; }

                [JsonProperty(PropertyName = "user_name")]
                public string UserName { get; set; }

                [JsonProperty(PropertyName = "user_rights")]
                public int UserRights { get; set; }

                [JsonProperty(PropertyName = "user_groups")]
                public object[] UserGroups { get; set; }

                [JsonProperty(PropertyName = "message_id")]
                public int MessageId { get; set; }

                [JsonProperty(PropertyName = "timestamp")]
                public int Timestamp { get; set; }

                [JsonProperty(PropertyName = "text")]
                public string Text { get; set; }

                public override string ToString()
                {
                    return $"{UserName}: {Text}";
                }
            }
        }
    }
}
