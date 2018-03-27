using StreamingServices.GoodGame.Models;
using System.Runtime.Serialization;

namespace StreamingServices.GoodGame.Chat.Models
{
    [DataContract]
    public class ResGGChannelHistory : GGMessage<ResGGChannelHistory.DataContainer>
    {
        [DataContract]
        public class DataContainer
        {
            [DataMember(Name = "channel_id")]
            public int ChannelId { get; set; }

            [DataMember(Name = "messages")]
            public Message[] Messages { get; set; }

            [DataContract]
            public class Message
            {
                [DataMember(Name = "user_id")]
                public int UserId { get; set; }

                [DataMember(Name = "user_name")]
                public string UserName { get; set; }

                [DataMember(Name = "user_rights")]
                public int UserRights { get; set; }

                [DataMember(Name = "user_groups")]
                public object[] UserGroups { get; set; }

                [DataMember(Name = "message_id")]
                public int MessageId { get; set; }

                [DataMember(Name = "timestamp")]
                public int Timestamp { get; set; }

                [DataMember(Name = "text")]
                public string Text { get; set; }

                public override string ToString()
                {
                    return $"{UserName}: {Text}";
                }
            }
        }
    }
}
