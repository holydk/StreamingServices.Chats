using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StreamingServices.GoodGame.Chat.Models;
using StreamingServices.GoodGame.Models;
using StreamingServices.Helpers;
using StreamingServices.Models;
using System.Dynamic;
using System.Reflection;
using System.Threading.Tasks;

namespace StreamingServices.GoodGame.Chat
{
    // https://github.com/GoodGame/API/tree/master/Chat

    /// <summary>
    /// Api of GoodGame Chat
    /// </summary>
    public class GoodGameChat : ChatClient<GoodGameRequest>
    {
        #region Members

        /// <summary>
        /// User id in chat
        /// </summary>
        private int _userId = 0;

        #endregion

        public GoodGameChat(string uriString)
            : base(uriString)
        { }

        /// <summary>
        /// Creates an instance of the StreamingServices.GoodGame.Chat.GoodGameChat
        /// </summary>
        /// <param name="uriString">Uri string in format: wss://{...}</param>
        /// <param name="userId">User id</param>
        /// <param name="token">Token</param>
        public GoodGameChat(string uriString, int userId, string token)
            : base(uriString)
        {
            _userId = userId;
            _token = token;
        }

        #region Public api of service

        public override Task AuthAsync() => AuthAsync(_userId, _token);

        public async Task AuthAsync(int userId, string token, int siteId = 1)
        {
            var req = GetCommand(GoodGameRequest.ChatAuthorization);

            req.data.site_id = siteId;
            req.data.user_id = userId;
            req.data.token = token;

            await SendAsync(req).ConfigureAwait(false);
        }

        public async Task JoinAsync(int channelId, bool hidden = false)
        {
            if (IsJoined &&
                _channel.Id == channelId)
            {
                OnError(new ChatErrorEventArgs(
                    "You try to connected to the same channel.",
                    ChatError.ConnectionToSameChannel));

                return;
            }

            await UnJoinAsync().ConfigureAwait(false);
            await InternalJoinAsync(channelId, hidden).ConfigureAwait(false);
        }

        public async Task GetChannelHistory(int channelId, int? from = null)
        {
            var req = GetCommand(GoodGameRequest.ChannelHistory);

            req.data.channel_id = channelId;
            req.data.from = from;

            await SendAsync(req).ConfigureAwait(false);
        }

        public override async Task SendMessageAsync(string text)
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

            var req = GetCommand(GoodGameRequest.SendMessage);

            req.data.channel_id = _channel?.Id ?? 0;
            req.data.hideIcon = false;
            req.data.mobile = false;
            req.data.text = text;              

            await SendAsync(req).ConfigureAwait(false);           
        }

        public async Task UnJoinAsync()
        {           
            if (!IsJoined)
                return;

            var req = GetCommand(GoodGameRequest.UnJoin);

            req.data.channel_id = _channel.Id;             

            await SendAsync(req).ConfigureAwait(false);
        }

        #endregion

        protected override Task PingAsync(object sender) => 
            SendAsync(GetCommand(GoodGameRequest.Ping));

        protected override Task PongAsync() =>
            SendAsync(GetCommand(GoodGameRequest.Pong));

        protected async override Task OnReconnectedAsync()
        {
            await base.OnReconnectedAsync().ConfigureAwait(false);

            if (IsConnected && IsJoined)
            {
                await Task.Delay(10).ConfigureAwait(false);
                await InternalJoinAsync(_channel.Id).ConfigureAwait(false);
            }
        }

        private async Task InternalJoinAsync(int channelId, bool hidden = false)
        {
            var req = GetCommand(GoodGameRequest.Join);

            req.data.channel_id = channelId;
            req.data.hidden = hidden;

            await SendAsync(req).ConfigureAwait(false);
        }

        protected override dynamic GetRequestModel(GoodGameRequest commandType)
        {
            dynamic request = new ExpandoObject();
            request.type = commandType.GetDescription();
            request.data = new ExpandoObject();

            return request;
        }

        protected override byte[] SerializeRequest(object model)
        {
            var json = JsonConvert.SerializeObject(
                model,
                Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        protected override void OnReceived(byte[] data)
        {
            var json = System.Text.Encoding.UTF8.GetString(data);
            var jsonModel = JObject.Parse(json);
            var reqType = jsonModel["type"].ToString();

            switch (reqType)
            {
                case "success_join":

                    var chatJoin = jsonModel.ToObject<ResGGChatJoin>();
                    
                    _channel = new Channel(
                        chatJoin.Data.ChannelId,
                        chatJoin.Data.ChannelName);
                    
                    break;

                case "success_auth":

                    var chatAuth = jsonModel.ToObject<ResGGChatAuth>();
                                      
                    _userName = chatAuth.Data.UserName;
                    _isAuthorized = true;

                    var req = GetCommand(GoodGameRequest.ChatAuthorization);

                    if (ExpandoHelper.TryGetValue(req.data, "token", out dynamic token))
                        _token = token;                   

                    break;

                case "message":

                    var msg = jsonModel.ToObject<ResGGSendMessage>();
                   
                    var index = msg.Data.Color.IndexOf('-');                    
                    var color = typeof(GoodGameColor)
                        .GetField(
                        index == -1 ? msg.Data.Color : msg.Data.Color.Remove(index, 1),
                        BindingFlags.GetProperty |
                        BindingFlags.IgnoreCase |
                        BindingFlags.Public |
                        BindingFlags.Static)
                        ?.GetValue(null) ?? GoodGameColor.Simple;

                    OnMessage(new ChatMessageEventArgs(
                        msg.Data.Text,
                        msg.Data.UserName,
                        (System.Drawing.Color)color));

                    break;

                case "success_unjoin":

                    var unJoin = jsonModel.ToObject<ResGGUnJoin>();

                    if (unJoin.Data.ChannelId == _channel.Id)                    
                        _channel = null;                   

                    break;

                default:
                    break;
            }
        }
    }
}
