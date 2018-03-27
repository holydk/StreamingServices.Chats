using System.ComponentModel;

namespace StreamingServices.GoodGame
{
    public enum GoodGameRequest
    {
        [Description("auth")]
        ChatAuthorization,

        [Description("get_channel_history")]
        ChannelHistory,

        [Description("send_message")]
        SendMessage,

        [Description("join")]
        Join,

        [Description("unjoin")]
        UnJoin,

        [Description("ping")]
        Ping,

        [Description("pong")]
        Pong 
    }
}
