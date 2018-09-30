using System.ComponentModel;

namespace StreamingServices.IRC
{
    public enum IRCMethod
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
        Part,

        [Description("PRIVMSG")]
        PrivMsg
    }
}
