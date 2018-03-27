using Newtonsoft.Json;
using StreamingServices.GoodGame.Models;

namespace StreamingServices.GoodGame.Chat.Models
{
    public class ResGGWelcome : GGMessage<ResGGWelcome.DataContainer>
    {
        public class DataContainer
        {
            [JsonProperty(PropertyName = "protocolVersion")]
            public float ProtocolVersion { get; set; }

            [JsonProperty(PropertyName = "serverIdent")]
            public string ServerIdent { get; set; }
        }
    }
}
