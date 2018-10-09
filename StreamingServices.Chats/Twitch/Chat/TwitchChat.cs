using StreamingServices.Chats.Abstractions;
using StreamingServices.Chats.Helpers;
using StreamingServices.Chats.IRC;
using StreamingServices.Chats.Models;
using StreamingServices.Helpers;
using StreamingServices.Utils.Abstractions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StreamingServices.Chats.Twitch.Chat
{
    public class TwitchChat : WebSocketChat
    {
        #region Private Fields

        private IIRCParser _parser;

        private string[] _colors;

        #endregion

        #region Constructors

        public TwitchChat(IWebSocketClient client, IIRCParser parser, ILogFactory logger = null)
            : base(client, logger)
        {
            _parser = parser;
            _colors = new string[] 
            {
                "#FF0000", "#0000FF", "#008000",
                "#FF7F50", "#B22222", "#9ACD32",
                "#FF4500", "#2E8B57", "#DAA520",
                "#D2691E", "#5F9EA0", "#1E90FF",
                "#FF69B4", "#8A2BE2", "#00FF7F"
            };
        }

        #endregion

        protected async override Task AuthorizeAsync(IChatCredential credential)
        {
            if (credential == null)
                throw new ArgumentNullException(nameof(credential));

            var messages = new IRCMessage[]
            {
                new IRCMessage(
                    command: "CAP",
                    middle: new[] { "REQ" },
                    trailing: "twitch.tv/tags twitch.tv/commands"),
                new IRCMessage(
                    command: "PASS",
                    middle: new[] { credential.Token.Unsecure() }),
                new IRCMessage(
                    command: "NICK",
                    middle: new[] { credential.User }),
                new IRCMessage(
                    command: "USER",
                    middle: new[] 
                    {
                        credential.User,
                        "8",
                        "*"
                    },
                    trailing: credential.User),
            };

            foreach (var message in messages)
            {
                await SendAsync(message).ConfigureAwait(false); 
            }
        }

        protected async override Task InternalJoinChannel(Channel channel)
        {
            var channelName = channel.Name.ToLower();

            // Check channel.Name as lowercase string
            if (Channels.Any(c => c.Name == channelName))
            {
                OnError(new ChatErrorEventArgs(
                    $"The attempt to connect to the same channel - {channel} in chat.",
                    ChatError.ConnectionToSameChannel));
            }
            else
            {
                var reqJoin = new IRCMessage(
                    command: "JOIN",
                    middle: new[] { $"#{channelName}" });

                await SendAsync(reqJoin).ConfigureAwait(false);
            }           
        }

        protected override Task InternalUnJoinChannel(Channel channel)
        {
            var reqPart = new IRCMessage(
                command: "PART",
                middle: new[] { $"#{channel.Name}" });

            return SendAsync(reqPart);
        }

        protected override Task InternalSendMessageAsync(string text, Channel channel)
        {
            var reqMsg = new IRCMessage(
                command: "PRIVMSG",
                middle: new[] { $"#{channel.Name}" },
                trailing: text);

            return SendAsync(reqMsg);
        }

        protected override void OnMessage(string msg)
        {
            var messages = _parser.Deserialize(msg);

            foreach (var message in messages)
            {
                switch (message.Command)
                {
                    case "GLOBALUSERSTATE":
                        HandleSuccessAuth(message);
                        break;

                    case "JOIN":
                        HandleSuccessJoin(message);
                        break;

                    case "PART":
                        HandleSuccessUnJoin(message);
                        break;

                    case "PING":
                        HandlePing();
                        break;

                    case "PRIVMSG":
                        HandleMessage(message);
                        break;

                    default:
                        break;
                }
            }
        }

        protected override void OnMessage(byte[] data)
        {
            
        }

        private void HandleSuccessAuth(IRCMessage message)
        {
            if (message == null)
                return;

            UserName = message.Tags["display-name"];
            IsAuthorized = true;
        }

        private void HandleSuccessJoin(IRCMessage message)
        {
            if (message == null)
                return;

            AddChannel(new Channel(null, message.Middle[0].Remove(0, 1)));
        }

        private void HandleSuccessUnJoin(IRCMessage message)
        {
            if (message == null)
                return;

            var channelName = message.Middle[0].Remove(0, 1);
            var channel = Channels.FirstOrDefault(c => c.Name == channelName);

            if (channel != null)
                RemoveChannel(channel);
        }

        private void HandleMessage(IRCMessage message)
        {
            if (message == null)
                return;

            var channelName = message.Middle[0].Remove(0, 1);
            var channel = Channels.FirstOrDefault(c => c.Name == channelName);
            var text = message.Trailing;
            var userName = message.Tags["display-name"];
            var color = GetColor(message.Tags["color"]);

            if (channel != null)           
                OnMessage(new ChatMessageEventArgs(text, userName, color, channel));           
        }

        private void HandlePing()
        {
            Task.Factory.StartNew(async () =>
            {
                await PongAsync().ConfigureAwait(false);
            }, TaskCreationOptions.RunContinuationsAsynchronously);
        }

        protected override Task PingAsync()
        {
            return SendAsync(new IRCMessage("PING"));
        }

        protected override Task PongAsync()
        {
            return SendAsync(new IRCMessage("PONG"));
        }

        private Task SendAsync(IRCMessage msg)
        {
            return SendAsync(_parser.Serialize(msg));
        }

        private string GetColor(string color)
        {
            if (!string.IsNullOrWhiteSpace(color))
                return color;

            var random = RandomProvider.GetThreadRandom();
            var colorIndex = random.Next(0, _colors.Length - 1);

            return _colors[colorIndex];
        }
    }
}
