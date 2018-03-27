using Newtonsoft.Json;
using StreamingServices.GoodGame.Models;

namespace StreamingServices.GoodGame.Chat.Models
{
    public class ResGGChatAuth : GGMessage<ResGGChatAuth.DataContainer>
    {
        public class DataContainer
        {
            [JsonProperty(PropertyName = "user_id")]
            public int UserId { get; set; }

            [JsonProperty(PropertyName = "user_name")]
            public string UserName { get; set; }
        }
    }
}

