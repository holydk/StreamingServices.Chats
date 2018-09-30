using Newtonsoft.Json;

namespace StreamingServices.GoodGame.Chat.Models
{
    internal class ResGGWelcome
    {
        [JsonProperty(PropertyName = "protocolVersion")]
        public float ProtocolVersion { get; set; }

        [JsonProperty(PropertyName = "serverIdent")]
        public string ServerIdent { get; set; }
    }
}
