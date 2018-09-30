using StreamingServices.Chats.Abstractions;
using StreamingServices.Chats.WebSockets;
using StreamingServices.Utils.Abstractions;
using StreamingServices.Utils.Logging;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StreamingServices.Chats.Models
{
    /// <summary>
    /// Base class for Chat
    /// </summary>
    public abstract class Chat : IChat
    {
        #region Private Fields

        private ILogFactory _logger;

        private bool _disposedValue = false;

        private IChatCredential _credential;

        #endregion

        #region Public Properties

        public abstract bool IsConnected { get; }

        public bool IsJoined { get; protected set; }

        public bool IsAuthorized { get; protected set; }

        public string UserName { get; protected set; }

        public IChannel Channel { get; protected set; }

        #endregion

        #region Events

        public event EventHandler<ChatErrorEventArgs> Error;

        public event EventHandler Connected;

        public event EventHandler Close;

        public event EventHandler<ChatMessageEventArgs> Message;

        #endregion

        #region Constructors

        public Chat(ILogFactory logger = null)
        {
            _logger = logger;
        }

        #endregion

        public Task ConnectAsync(Uri uri, IChatCredential credential)
        {
            CheckDisposing();

            _credential = credential;

            return ConnectAsync(uri);
        }

        public virtual Task CloseAsync()
        {
            CheckDisposing();

            Channel = null;
            UserName = null;
            IsAuthorized = false;
            _credential = null;

            return Task.CompletedTask;
        }

        protected abstract Task SendAsync(string msg);

        protected abstract Task ConnectAsync(Uri uri);

        protected abstract Task AuthorizeAsync(IChatCredential credential);

        protected abstract Task PingAsync();

        protected abstract Task PongAsync();

        public abstract Task SendMessageAsync(string text);

        public abstract Task JoinChannel(IChannel channel);

        public abstract Task UnJoinChannel(IChannel channel);

        protected virtual void OnError(ChatErrorEventArgs e)
        {
            Error?.Invoke(this, e);
        }
            
        protected virtual void OnClose(EventArgs e)
        {
            Close?.Invoke(this, e);
        }

        protected virtual void OnConnected(EventArgs e)
        {
            IsJoined = false;
            IsAuthorized = false;

            Task.Factory.StartNew(async () =>
            {
                await AuthorizeAsync(_credential).ConfigureAwait(false);
            }, TaskCreationOptions.RunContinuationsAsynchronously);
            
            Connected?.Invoke(this, e);
        }

        protected virtual void OnMessage(ChatMessageEventArgs e)
        {
            Message?.Invoke(this, e);
        }

        protected void LogError(string msg, ChatError? type = null)
        {
            _logger?.Log(msg, LogLevel.Error);

            if (type.HasValue)
            {
                OnError(new ChatErrorEventArgs(msg, type.Value));
            }
        }

        #region IDisposable Support

        protected void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    DisposeManagementResources();
                }

                DisposeUnManagementResources();

                _disposedValue = true;
            }
        }

        protected virtual void DisposeUnManagementResources()
        {
            
        }

        protected virtual void DisposeManagementResources()
        {
            _logger = null;
            Channel = null;
            UserName = null;
            IsAuthorized = false;
            _credential = null;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected void CheckDisposing()
        {
            if (_disposedValue)
            {
                var objName = nameof(WebSocketClient);

                _logger?.Log($"{objName} is disposed.", LogLevel.Error);

                throw new ObjectDisposedException(objName);
            }
        }

        #endregion
    }

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

        protected abstract void OnMessage(string msg);

        protected abstract void OnMessage(byte[] data);

        protected override Task ConnectAsync(Uri uri)
        {
            return _client.ConnectAsync(uri);
        }

        public async override Task CloseAsync()
        {
            await base.CloseAsync().ConfigureAwait(false);

            if (!IsConnected)
                return;

            await _client.CloseAsync().ConfigureAwait(false);
        }

        private void OnConnected(object client, EventArgs e)
        {
            //_pingTimer?.Dispose();
            // TODO: Исключения в сокете из за потоков
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

        protected async override Task SendAsync(string msg)
        {
            CheckDisposing();

            if (string.IsNullOrWhiteSpace(msg))
                return;

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
