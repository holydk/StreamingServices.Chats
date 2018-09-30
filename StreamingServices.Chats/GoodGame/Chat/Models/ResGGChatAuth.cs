using Newtonsoft.Json;

namespace StreamingServices.GoodGame.Chat.Models
{
    internal class ResGGChatAuth
    {
        [JsonProperty(PropertyName = "user_id")]
        public int UserId { get; set; }

        [JsonProperty(PropertyName = "user_name")]
        public string UserName { get; set; }
    }
}

