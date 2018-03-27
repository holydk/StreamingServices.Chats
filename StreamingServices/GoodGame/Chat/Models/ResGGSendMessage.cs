using StreamingServices.GoodGame.Models;
using System.Runtime.Serialization;

namespace StreamingServices.GoodGame.Chat.Models
{
    [DataContract]
    public class ResGGSendMessage : GGMessage<ResGGSendMessage.DataContainer>
    {
        [DataContract]
        public class DataContainer
        {
            [DataMember(Name = "channel_id")]
            public int ChannelId { get; set; }

            [DataMember(Name = "user_id")]
            public int UserId { get; set; }

            [DataMember(Name = "user_name")]
            public string UserName { get; set; }

            [DataMember(Name = "message_id")]
            public int MessageId { get; set; }

            [DataMember(Name = "timestamp")]
            public int Timestamp { get; set; }

            [DataMember(Name = "text")]
            public string Text { get; set; }

            [DataMember(Name = "hideIcon")]
            public bool HideIcon { get; set; }

            [DataMember(Name = "mobile")]
            public bool Mobile { get; set; }

            [DataMember(Name = "color")]
            public string Color { get; set; }
        }

        public override string ToString()
        {
            return $"{Data.UserName}: {Data.Text}";
        }
    }
}
