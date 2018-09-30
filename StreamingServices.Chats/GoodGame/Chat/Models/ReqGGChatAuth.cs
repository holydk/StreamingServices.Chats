using Newtonsoft.Json;

namespace StreamingServices.Chats.GoodGame.Chat.Models
{
    internal class ReqGGChatAuth
    {
        [JsonProperty(PropertyName = "user_id")]
        public int UserId { get; set; }

        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }
    }
}
