using Newtonsoft.Json;

namespace StreamingServices.Chats.GoodGame.Chat.Models
{
    public class ReqGGPong
    {
        [JsonProperty(PropertyName = "answer")]
        public string Answer { get; set; } = "pong";
    }
}
