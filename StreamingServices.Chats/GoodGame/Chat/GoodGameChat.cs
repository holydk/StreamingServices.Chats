using StreamingServices.Chats.Abstractions;
using StreamingServices.Chats.GoodGame.Chat.Models;
using StreamingServices.Chats.Helpers;
using StreamingServices.Chats.Models;
using StreamingServices.GoodGame.Chat.Models;
using StreamingServices.GoodGame.Models;
using StreamingServices.Utils.Abstractions;
using System;
using System.Linq;
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
                    throw new InvalidOperationException(
                        "Invalid conversion IChatCredential.User to int.");
                }
            }

            return SendModelAsync(model);
        }

        protected async override Task InternalJoinChannel(Channel channel)
        {
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

                await SendModelAsync(model).ConfigureAwait(false);
            }
            else
            {
                throw new InvalidOperationException("ChannelId is null.");
            }
        }

        protected override Task InternalUnJoinChannel(Channel channel)
        {
            var model = new GGMessage<ResGGChatUnJoin>
            {
                Type = "unjoin",
                Data =
                {
                    ChannelId = channel.Id.Value
                }
            };

            return SendModelAsync(model);
        }

        protected override Task InternalSendMessageAsync(string text, Channel channel)
        {
            // TODO : add IChatOptions
            var model = new GGMessage<ReqGGSendMessage>()
            {
                Type = "send_message",
                Data =
                {
                    ChannelId = channel.Id.Value,
                    Mobile = 0,
                    Text = text
                    // icon = ???
                    // mobile = ???
                    // color = ???
                }
            };

            return SendModelAsync(model);
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

                    HandleSuccessAuth(msg);

                    break;

                case "success_join":

                    HandleSuccessJoin(msg);

                    break;

                case "success_unjoin":

                    HandleSuccessUnJoin(msg);

                    break;

                case "message":

                    HandleMessage(msg);

                    break;

                case "ping":

                    HandlePing(msg);

                    break;

                default: break;
            }
        }

        protected override void OnMessage(byte[] data)
        {
            
        }

        private void HandleSuccessAuth(string json)
        {
            var authModel = Deserialize<ResGGChatAuth>(json);
            if (authModel == null)
                return;

            UserName = authModel.UserName;
            IsAuthorized = true;
        }

        private void HandleSuccessJoin(string json)
        {
            var joinModel = Deserialize<ResGGChatJoin>(json);
            if (joinModel == null)
                return;

            AddChannel(new Channel(joinModel.ChannelId, joinModel.ChannelName));
        }

        private void HandleSuccessUnJoin(string json)
        {
            var unJoinModel = Deserialize<ResGGChatUnJoin>(json);
            if (unJoinModel == null)
                return;

            var channel = Channels.FirstOrDefault(c => c.Id == unJoinModel.ChannelId);
            if (channel != null)
                RemoveChannel(channel);
        }

        private void HandleMessage(string json)
        {
            var msgModel = Deserialize<ResGGSendMessage>(json);
            if (msgModel == null)
                return;

            var channel = Channels.FirstOrDefault(c => c.Id == msgModel.ChannelId);
            if (channel == null)
            {
                OnError(new ChatErrorEventArgs(
                    $"The response from unknown channel with Id - {msgModel.ChannelId}.",
                    ChatError.NotFoundChannel));
            }
            else
            {
                OnMessage(new ChatMessageEventArgs(
                    msgModel.Text,
                    msgModel.UserName,
                    GetUserColorByName(msgModel.Color),
                    channel));
            }
        }

        private void HandlePing(string json)
        {
            Task.Factory.StartNew(async () =>
            {
                await PongAsync().ConfigureAwait(false);
            }, TaskCreationOptions.RunContinuationsAsynchronously);
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

        private T Deserialize<T>(string json)
            where T : class, new()
        {
            return _parser.Deserialize<GGMessage<T>>(json)?.Data;
        }

        private string GetUserColorByName(string name)
        {
            switch (name)
            {
                case "bronze": return "#e7820a";
                case "silver": return "#b4b4b4";
                case "gold": return "#eefc08";
                case "diamond": return "#8781bd";
                case "king": return "#30d5c8";
                case "top-one": return "#3BCBFF";
                case "streamer": return "#e8bb00";
                case "moderator": return "#ec4058";
                case "premium-personal": return "#31a93a";
                default: return "#73adff";
            }
        }
    }
}
