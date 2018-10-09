using StreamingServices.Chats.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace StreamingServices.Chats.IRC
{
    public class IRCParser : IIRCParser
    {
        #region Private Fields

        private Regex _ircMsgRegex = new Regex(@"^(?:@(?<Tags>[^ ]+) )?(?::(?<Prefix>" +
            @"[^ ]+) )?(?:(?<Command>[^ :]+) ?)(?<Middle>((?!:)\S+ ?)*)?(?: :(?<Trailing>.+))?$",
            RegexOptions.Multiline);
        // ^(?i)(?!:)\w+
        // (^(?!:))?
        // (?<!^:)
        // (?<!:)
        private const string Tags = "Tags";
        private const string Prefix = "Prefix";
        private const string Command = "Command";
        private const string Middle = "Middle";
        private const string Trailing = "Trailing";

        #endregion

        #region IIRCParser Interface Methods

        public IRCMessage[] Deserialize(string msg)
        {
            if (string.IsNullOrWhiteSpace(msg))
                return null;

            var messages = new HashSet<IRCMessage>();

            foreach (Match match in _ircMsgRegex.Matches(msg))
            {
                if (!match.Success) continue;

                messages.Add(new IRCMessage(
                    GetIRCPartByName(match, Command), 
                    GetTags(match),
                    GetIRCPartByName(match, Prefix),
                    GetMiddle(match),
                    GetIRCPartByName(match, Trailing)));
            }

            return messages.ToArray();
        }

        public string Serialize(IRCMessage msg)
        {
            if (msg == null)
                return null;

            if (string.IsNullOrWhiteSpace(msg.Command))
                throw new InvalidOperationException(
                    "The IRC message didn't contain the command part.");

            var ircBuilder = new StringBuilder();
            var tags = GetTagsAsString(msg);

            if (tags != null)
                ircBuilder.AppendFormat("{0} ", tags);

            if (!string.IsNullOrWhiteSpace(msg.Prefix))
                ircBuilder.Append($":{msg.Prefix} ");

            ircBuilder.Append(msg.Command);

            var middle = GetMiddleAsString(msg);

            if (!string.IsNullOrEmpty(middle))
                ircBuilder.Append($" {middle}");

            if (!string.IsNullOrWhiteSpace(msg.Trailing))
                ircBuilder.Append($" :{msg.Trailing}");

            return ircBuilder.ToString();
        } 

        #endregion

        private string GetTagsAsString(IRCMessage msg)
        {
            if (msg.Tags == null || !msg.Tags.Any())
                return null;

            var tagsBuilder = new StringBuilder("@");

            foreach (var tag in msg.Tags)
            {
                tagsBuilder.Append($"{tag.Key}={tag.Value};");
            }

            tagsBuilder.Remove(tagsBuilder.Length - 1, 1);

            return tagsBuilder.ToString();
        }

        private Dictionary<string, string> GetTags(Match match)
        {
            var tagsPart = GetIRCPartByName(match, Tags);

            if (string.IsNullOrWhiteSpace(tagsPart))
                return null;

            var tagsAsString = GetIRCPartByName(match, Tags).Split(';');
            var tags = new Dictionary<string, string>();

            foreach (var tag in tagsAsString)
            {
                tags.Add(tag.Split('=')[0], tag.Split('=')[1]);
            }

            return tags;
        }

        private string GetMiddleAsString(IRCMessage msg)
        {
            if (msg.Middle == null || !msg.Middle.Any())
                return null;

            if (msg.Middle.Length == 1)
                return msg.Middle[0];

            return string.Join(" ", msg.Middle);
        }

        private string[] GetMiddle(Match match)
        {
            var middlePart = GetIRCPartByName(match, Middle);

            if (string.IsNullOrWhiteSpace(middlePart))
                return null;

            return middlePart.Split(' ');
        }

        private string GetIRCPartByName(Match match, string partName)
        {
            var partValue = match.Groups[partName].Value;

            if (string.IsNullOrWhiteSpace(partValue))
                return null;

            return partValue.TrimEnd(new[] { '\r', '\n' });
        }
    }
}
