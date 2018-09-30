using System.Collections.Generic;

namespace StreamingServices.Chats.IRC
{
    public class IRCMessage
    {
        #region Public Properties

        public Dictionary<string, string> Tags { get; }

        public string Prefix { get; }

        public string Command { get; }

        public string[] Middle { get; }

        public string Trailing { get; }

        #endregion

        #region Constructors

        public IRCMessage(
            Dictionary<string, string> tags, 
            string prefix, 
            string command, 
            string[] middle,
            string trailing)
        {
            Tags = tags;
            Prefix = prefix;
            Command = command;
            Middle = middle;
            Trailing = trailing;
        }

        #endregion
    }
}
