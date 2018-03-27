using StreamingServices.GoodGame.Models;
using System.Runtime.Serialization;

namespace StreamingServices.GoodGame.Chat.Models
{
    [DataContract]
    public class ResGGChatAuth : GGMessage<ResGGChatAuth.DataContainer>
    {
        [DataContract]
        public class DataContainer
        {
            [DataMember(Name = "user_id")]
            public int UserId { get; set; }

            [DataMember(Name = "user_name")]
            public string UserName { get; set; }
        }
    }
}
