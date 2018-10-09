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
            string command,
            Dictionary<string, string> tags = null, 
            string prefix = null,
            string[] middle = null,
            string trailing = null)
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
