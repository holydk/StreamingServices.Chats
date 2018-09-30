using StreamingServices.Chats.Abstractions;
using StreamingServices.Chats.GoodGame.Chat.Models;
using StreamingServices.Chats.Helpers;
using StreamingServices.Chats.Models;
using StreamingServices.GoodGame.Chat.Models;
using StreamingServices.GoodGame.Models;
using StreamingServices.Utils.Abstractions;
using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StreamingServices.Chats.GoodGame.Chat
{
    public class GoodGameChat : WebSocketChat
    {
        #region Private Fields

        private IJsonParser _parser;

        private Regex _msgTypeRegex = new Regex("{\"type\":\"(?<Type>[^\"]+)\".+");

        #endregion

        #region Constructors

        public GoodGameChat(IWebSocketClient client, IJsonParser parser, ILogFactory logger = null)
            : base(client, logger)
        {
            _parser = parser;
        }

        #endregion

        protected override Task AuthorizeAsync(IChatCredential credential)
        {
            var model = new GGMessage<ReqGGChatAuth>
            {
                Type = "auth"
            };

            if (credential == null)
            {
                model.Data.UserId = 0;
            }
            else
            {
                if (int.TryParse(credential.User, out int userId))
                {
                    model.Data.UserId = userId;
                    model.Data.Token = credential.Token.Unsecure();
                }
                else
                {
                    var msg = "Invalid conversion IChatCredential.User as int.";

                    LogError(msg);

                    throw new InvalidOperationException(msg);
                }
            }

            return SendModelAsync(model);
        }

        public async override Task JoinChannel(IChannel channel)
        {
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }

            if (!IsConnected)
            {
                OnError(new ChatErrorEventArgs(
                    "You not connected to chat.",
                    ChatError.InvalidConnection));

                return;
            }

            if (!IsAuthorized)
            {
                OnError(new ChatErrorEventArgs(
                    "You not authorized in chat.",
                    ChatError.NotAuthorized));

                return;
            }

            if (Channel != null && Channel.Equals(channel))
            {
                OnError(new ChatErrorEventArgs(
                    "You connected to the same channel in chat.",
                    ChatError.ConnectionToSameChannel));

                return;
            }

            if (channel.Id.HasValue)
            {
                var model = new GGMessage<ReqGGChatJoin>
                {
                    Type = "join",
                    Data =
                    {
                        ChannelId = channel.Id.Value,
                        Hidden = 0
                    }
                };

                Channel = channel;

                await SendModelAsync(model).ConfigureAwait(false);
            }
            else
            {
                var msg = "ChannelId is null.";

                LogError(msg);

                throw new InvalidOperationException(msg);
            }
        }

        public async override Task UnJoinChannel(IChannel channel)
        {
            if (!IsJoined)
            {
                OnError(new ChatErrorEventArgs(
                    "You not joined to channel.",
                    ChatError.NotJoinedToChannel));

                return;
            }

            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }

            if (!IsConnected)
            {
                OnError(new ChatErrorEventArgs(
                    "You not connected to chat.",
                    ChatError.InvalidConnection));

                return;
            }

            if (!IsAuthorized)
            {
                OnError(new ChatErrorEventArgs(
                    "You not authorized in chat.",
                    ChatError.NotAuthorized));

                return;
            }

            if (Channel.Equals(channel))
            {
                var model = new GGMessage<GGUnJoin>
                {
                    Type = "unjoin",
                    Data =
                    {
                        ChannelId = channel.Id.Value
                    }
                };

                await SendModelAsync(model).ConfigureAwait(false);
            }
        }

        public async override Task SendMessageAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            if (!IsConnected)
            {
                OnError(new ChatErrorEventArgs(
                    "You not connected to chat.",
                    ChatError.InvalidConnection));

                return;
            }

            if (!IsAuthorized)
            {
                OnError(new ChatErrorEventArgs(
                    "You not authorized in chat.",
                    ChatError.NotAuthorized));

                return;
            }

            if (!IsJoined)
            {
                OnError(new ChatErrorEventArgs(
                    "You not joined to channel.",
                    ChatError.NotJoinedToChannel));

                return;
            }

            // TODO : add IChatOptions
            var model = new GGMessage<ReqGGSendMessage>()
            {
                Type = "send_message",
                Data =
                {
                    ChannelId = Channel.Id.Value,
                    Mobile = 0,
                    Text = text
                    // icon = ???
                    // mobile = ???
                    // color = ???
                }
            };

            await SendModelAsync(model).ConfigureAwait(false);
        }

        private Task SendModelAsync(object model)
        {
            return SendAsync(_parser.Serialize(model));
        }

        protected override void OnMessage(string msg)
        {
            if (string.IsNullOrWhiteSpace(msg))
                return;

            switch (GetCommandType(msg))
            {
                case "success_auth":

                    var authModel = _parser
                        .Deserialize<GGMessage<ResGGChatAuth>>(msg);

                    if (authModel == null)
                        return;

                    UserName = authModel.Data.UserName;
                    IsAuthorized = true;

                    break;

                case "success_join":

                    var joinModel = _parser
                        .Deserialize<GGMessage<ResGGChatJoin>>(msg);

                    if (joinModel == null ||
                        Channel == null ||
                        !Channel.Id.HasValue)
                        return;

                    if (joinModel.Data.ChannelId == Channel.Id)
                        IsJoined = true;

                    break;

                case "success_unjoin":

                    break;

                case "message":

                    var msgModel = _parser
                        .Deserialize<GGMessage<ResGGSendMessage>>(msg);

                    if (msgModel == null)
                        return;

                    OnMessage(new ChatMessageEventArgs(
                        msgModel.Data.Text, 
                        msgModel.Data.UserName, 
                        // TODO: Update color
                        Color.AliceBlue));

                    break;

                case "ping":

                    Task.Factory.StartNew(async () =>
                    {
                        await PongAsync().ConfigureAwait(false);
                    }, TaskCreationOptions.RunContinuationsAsynchronously);

                    break;

                default: break;
            }
        }

        protected override void OnMessage(byte[] data)
        {
            
        }

        protected override Task PingAsync()
        {
            return SendModelAsync(new GGMessage<object>()
            {
                Type = "ping"
            });
        }

        protected override Task PongAsync()
        {
            return SendModelAsync(new GGMessage<ReqGGPong>()
            {
                Type = "pong"
            });
        }

        private string GetCommandType(string json)
        {
            if (!_msgTypeRegex.IsMatch(json))
                return null;

            return _msgTypeRegex.Match(json).Groups["Type"].Value;
        }
    }
}
