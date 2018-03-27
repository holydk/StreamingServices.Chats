using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using StreamingServices.Helpers;
using StreamingServices.Models;

namespace StreamingServices.Twitch.Chat
{
    public class TwitchChat : ChatClient<TwitchRequest>
    {
        private Regex _ircMsgRegex = new Regex(@"^(?:@(?<Properties>[^ ]+) )?(?::(?<Prefix>" +
            "[^ ]+) )?(?:(?<Command>[^ :]+) ?)(?:(?<Middle>[^ :]+) ?)*(?::(?<Trailing>.+)?)?",
            RegexOptions.Multiline);

        public TwitchChat(string url, string userName, string token)
            : base(url)
        {
            _userName = userName;
            _token = token;
            _defaultPingDelay = 1000 * 60;
        }

        public async override Task AuthAsync()
        {
            // CAP REQ :{params}
            var reqCap = GetCommand(TwitchRequest.Cap);

            reqCap.middle = "REQ";
            reqCap.trailing = "twitch.tv/tags twitch.tv/commands";

            await SendAsync(reqCap).ConfigureAwait(false);

            // PASS {token}
            var reqPass = GetCommand(TwitchRequest.Pass);

            reqPass.middle = _token;

            await SendAsync(reqPass).ConfigureAwait(false);

            // NICK {userName}
            var reqNick = GetCommand(TwitchRequest.Nick);

            reqNick.middle = _userName;

            await SendAsync(reqNick).ConfigureAwait(false);

            // USER {userName} 8 * :{userName}
            var reqUser = GetCommand(TwitchRequest.User);

            reqUser.middle = $"{_userName} 8 *";
            reqUser.trailing = _userName;

            await SendAsync(reqUser).ConfigureAwait(false);
        }

        public async Task JoinAsync(string channelName)
        {
            if (IsJoined &&
                _channel.Name == channelName.ToLower())
            {
                OnError(new ChatErrorEventArgs(
                    "You try to connected to the same channel.",
                    ChatError.ConnectionToSameChannel));

                return;
            }

            await UnJoinAsync().ConfigureAwait(false);
            await InternalJoinAsync(channelName).ConfigureAwait(false);
        }

        public async Task UnJoinAsync()
        {
            if (!IsJoined)
                return;

            var req = GetCommand(TwitchRequest.UnJoin);

            req.middle = $"#{_channel.Name}";

            await SendAsync(req).ConfigureAwait(false);
        }

        public async override Task SendMessageAsync(string text)
        {
            if (!_isAuthorized)
            {
                OnError(new ChatErrorEventArgs(
                    "You are not authorized.",
                    ChatError.NotAuthorized));

                return;
            }

            if (!IsJoined)
            {
                OnError(new ChatErrorEventArgs(
                    "You are not joined to channel.",
                    ChatError.NotJoinedToChannel));

                return;
            }

            var req = GetCommand(TwitchRequest.SendMessage);

            req.middle = $"#{_channel.Name}";
            req.trailing = text;

            await SendAsync(req).ConfigureAwait(false);
        }

        public Task SendColorMessageAsync(string text) =>
            SendMessageAsync($"/me {text}");

        protected async override Task OnReconnectedAsync()
        {
            await base.OnReconnectedAsync().ConfigureAwait(false);

            if (IsConnected && IsJoined)
            {
                await Task.Delay(10).ConfigureAwait(false);
                await InternalJoinAsync(_channel.Name).ConfigureAwait(false);
            }
        }

        private async Task InternalJoinAsync(string channelName)
        {
            var req = GetCommand(TwitchRequest.Join);

            req.middle = $"#{channelName.ToLower()}";

            await SendAsync(req).ConfigureAwait(false);
        }

        protected override Task PingAsync(object sender) =>
            SendAsync(GetCommand(TwitchRequest.Ping));

        protected override Task PongAsync() =>
            SendAsync(GetCommand(TwitchRequest.Pong));

        protected override byte[] SerializeRequest(object model)
        {
            if (model is IDictionary<string, object> req)
            {
                var ircBuilder = new StringBuilder();

                ircBuilder.Append(req["command"].ToString());

                if (req.ContainsKey("middle")) {
                    ircBuilder.Append($" {req["middle"]}");
                }

                if (req.ContainsKey("trailing")) {
                    ircBuilder.Append($" :{req["trailing"]}");
                }

                return Encoding.UTF8.GetBytes(ircBuilder.ToString());
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }

        protected override dynamic GetRequestModel(TwitchRequest commandType)
        {
            dynamic request = new ExpandoObject();
            request.command = commandType.GetDescription();

            return request;
        }

        protected override void OnReceived(byte[] data)
        {
            var ircMsg = Encoding.UTF8.GetString(data);

            foreach (Match match in _ircMsgRegex.Matches(ircMsg))
            {
                if (!match.Success) continue;

                var cmdName = match
                    .Groups["Command"]
                    .Value
                    .TrimEnd(new[] { '\r', '\n' });

                switch (cmdName)
                {
                    case "PRIVMSG":

                        // Example of properties
                        // badges=subscriber/12;
                        // color=#5F9EA0;
                        // display-name=qwerty;
                        // emotes=71845:26-32;
                        // id=b12345a-605d-4758-a123-6e7ac567c015;
                        // mod=0;
                        // room-id=30814134;
                        // subscriber=1;
                        // tmi-sent-ts=1521820442105;
                        // turbo=0;
                        // user-id=123456789
                        // user-type=
                        var properties = GetProperties(match);

                        var userName = GetValue(properties[2]);
                        var colorName = GetValue(properties[1]);
                        var msg = match.Groups["Trailing"].Value;

                        // Color of user
                        Color color;

                        // Twitch Colors
                        var colorFields = typeof(TwitchColor).GetFields(
                            BindingFlags.Static |
                            BindingFlags.Public |
                            BindingFlags.GetProperty);

                        if (!string.IsNullOrEmpty(colorName))
                        {
                            var colorField = 
                                (from field in colorFields
                                 where ((Color)field.GetValue(null)).Name == colorName
                                 select field).FirstOrDefault();

                            if (colorField == null)                            
                                color = Color.FromName(colorName);                           
                            else
                                color = (Color)colorField.GetValue(null);
                        }
                        else
                        {
                            var random = RandomProvider.GetThreadRandom();
                            var colorIndex = random.Next(0, colorFields.Length - 1);

                            color = (Color)colorFields[colorIndex].GetValue(null);
                        }

                        OnMessage(new ChatMessageEventArgs(msg, userName, color));

                        break;

                    case "GLOBALUSERSTATE":

                        // Example of properties
                        // badges=;
                        // color=#00FF7F;
                        // display-name=qwerty;
                        // emote-sets=0;
                        // user-id=123456789;
                        // user-type=
                        properties = GetProperties(match);

                        _userName = GetValue(properties[2]);
                        _isAuthorized = true;

                        break;

                    case "ROOMSTATE":

                        // Example of properties
                        // broadcaster-lang=;
                        // emote-only=0;
                        // followers-only=-1;
                        // r9k=0;
                        // rituals=0;
                        // room-id=123456789;
                        // slow=0;
                        // subs-only=0
                        properties = GetProperties(match);

                        var roomId = int.Parse(GetValue(properties[5]));
                        var roomName = match
                            .Groups["Middle"]
                            .Value
                            .TrimStart('#')
                            .ToLower();

                        _channel = new Channel(roomId, roomName);

                        break;

                    case "PART":

                        _channel = null;

                        break;

                    case "PING":

                        PongAsync().GetAwaiter().GetResult();

                        break;

                    default:
                        break;
                }

                System.Console.WriteLine(match.Groups["Command"].Value);
            }
        }

        private string[] GetProperties(Match match) => 
            match.Groups["Properties"].Value.Split(';');

        private string GetValue(string property) =>
            property.Split('=')[1];
    }
}
