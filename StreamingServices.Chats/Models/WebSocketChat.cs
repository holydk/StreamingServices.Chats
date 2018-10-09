using StreamingServices.Chats.Abstractions;
using StreamingServices.Chats.WebSockets;
using StreamingServices.Utils.Abstractions;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StreamingServices.Chats.Models
{
    public abstract class WebSocketChat : Chat
    {
        #region Private Fields

        private IWebSocketClient _client;

        private Timer _pingTimer;

        private int _defaultPingDelay = 10000;

        #endregion

        #region Public Properties

        public override bool IsConnected => _client.IsConnected;

        #endregion

        #region Constructors

        public WebSocketChat(IWebSocketClient client, ILogFactory logger = null)
            : base(logger)
        {
            _client = client;
            _client.Connected += OnConnected;
            _client.Close += OnClose;
            _client.Error += OnError;
            _client.Message += OnMessage;
        }

        #endregion

        protected abstract Task PingAsync();

        protected abstract Task PongAsync();

        protected abstract void OnMessage(string msg);

        protected abstract void OnMessage(byte[] data);

        protected override Task ConnectAsync(Uri uri)
        {
            return _client.ConnectAsync(uri);
        }

        protected override Task InternalCloseAsync()
        {
            return _client.CloseAsync();
        }

        private void OnConnected(object client, EventArgs e)
        {
            if (_pingTimer == null)
                _pingTimer = new Timer(Ping, null, 0, _defaultPingDelay);

            OnConnected(e);
        }

        private void OnClose(object client, WebSocketCloseEventArgs e)
        {
            OnClose(EventArgs.Empty);
        }

        private void OnError(object client, WebSocketErrorEventArgs e)
        {
            if (!_client.IsConnected)
            {
                _pingTimer?.Dispose();
                _pingTimer = null;

                // Try to make reconnection 
                Task.Factory.StartNew(async () =>
                {
                    await _client.ReconnectAsync().ConfigureAwait(false);
                }, TaskCreationOptions.RunContinuationsAsynchronously);

                LogError(e.Message, ChatError.InvalidConnection);
            }
        }

        private void OnMessage(object client, WebSocketResponseEventArgs e)
        {
            switch (e.MessageType)
            {
                case WebSocketMessageType.Binary:

                    OnMessage(e.Data);

                    break;
                case WebSocketMessageType.Text:

                    OnMessage(Encoding.UTF8.GetString(e.Data));

                    break;

                default: break;
            }
        }

        protected override void DisposeManagementResources()
        {
            base.DisposeManagementResources();

            _client.Connected -= OnConnected;
            _client.Close -= OnClose;
            _client.Error -= OnError;
            _client.Message -= OnMessage;
            _client.Dispose();
            _client = null;

            _pingTimer?.Dispose();
            _pingTimer = null;
        }

        protected sealed async override Task InternalSendAsync(string msg)
        {
            var data = Encoding.UTF8.GetBytes(msg);

            await _client
                .SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text)
                .ConfigureAwait(false);
        }

        private void Ping(object state)
        {
            Task.Factory.StartNew(async () =>
            {
                await PingAsync().ConfigureAwait(false);
            }, TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}
