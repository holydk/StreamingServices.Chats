using System.ComponentModel;

namespace StreamingServices.Twitch
{
    public enum TwitchRequest
    {
        [Description("PASS")]
        Pass,

        [Description("NICK")]
        Nick,

        [Description("USER")]
        User,

        [Description("PING")]
        Ping,

        [Description("PONG")]
        Pong,

        [Description("CAP")]
        Cap,

        [Description("JOIN")]
        Join,

        [Description("PART")]
        UnJoin,

        [Description("PRIVMSG")]
        SendMessage
    }
}
