using StreamingServices.Chats.IRC;

namespace StreamingServices.Chats.Abstractions
{
    public interface IIRCParser
    {
        string Serialize(IRCMessage message);

        IRCMessage[] Deserialize(string msg);
    }
}
