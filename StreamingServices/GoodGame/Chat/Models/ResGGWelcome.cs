using StreamingServices.GoodGame.Models;
using System.Runtime.Serialization;

namespace StreamingServices.GoodGame.Chat.Models
{
    [DataContract]
    public class ResGGWelcome : GGMessage<ResGGWelcome.DataContainer>
    {
        [DataContract]
        public class DataContainer
        {
            [DataMember(Name = "protocolVersion")]
            public float ProtocolVersion { get; set; }

            [DataMember(Name = "serverIdent")]
            public string ServerIdent { get; set; }
        }
    }
}
