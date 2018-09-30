using Newtonsoft.Json;

namespace StreamingServices.GoodGame.Chat.Models
{
    internal class ResGGChatJoin
    {
        [JsonProperty(PropertyName = "channel_id")]
        public int ChannelId { get; set; }

        [JsonProperty(PropertyName = "channel_name")]
        public string ChannelName { get; set; }
        
        [JsonProperty(PropertyName = "access_rights")]
        public int AccessRights { get; set; }

        [JsonProperty(PropertyName = "is_banned")]
        public bool IsBanned { get; set; }
    }
}
