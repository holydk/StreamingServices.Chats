using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace StreamingServices.Models
{
    public abstract class ChatClient<TCommandType> : Chat<TCommandType>, IDisposable
        where TCommandType : struct, IConvertible
    {
        #region Members

        private ClientWebSocket _client;

        private int _reconnectionDelay = 5000;

        private readonly SemaphoreSlim _reconnectionSemph = new SemaphoreSlim(1, 1);

        private readonly object _locker = new object();

        private int _receiveBufferSize = 1024;

        private bool _disposed = false;

        private Timer _pingTimer;

        protected int _defaultPingDelay = 10000;

        /// <summary>
        /// Indicates whether there is a connection to the server socket
        /// </summary>
        public bool IsConnected
        {
            get
            {
                lock (_locker)
                {
                    if (_client == null ||
                        _client.State != WebSocketState.Open)
                        return false;
                    else
                        return true;
                }
            }
        }

        public ChatClient(string uriString)
            : base(uriString)
        { }

        #endregion

        protected abstract Task PingAsync(object sender);

        protected abstract Task PongAsync();

        protected abstract byte[] SerializeRequest(object model);

        protected abstract void OnReceived(byte[] data);

        /// <summary>
        /// Connect to web service
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            if (_disposed)
                throw new ObjectDisposedException(
                    "ClientWebSocketBase",
                    "Object was disposed.");

            if (IsConnected)            
                throw new ArgumentException(
                    "Connection already in progress.");            

            _client = new ClientWebSocket();

            try
            {
                await _client.ConnectAsync(
                        new Uri(_uriString),
                        CancellationToken.None)
                        .ConfigureAwait(false);
            }
            catch (InvalidOperationException)
            {
                OnError(new ChatErrorEventArgs(
                    "Connection error.",
                    ChatError.InvalidConnection));

                return;
            }
            catch (WebSocketException ex)
            {
                OnError(new ChatErrorEventArgs(
                    ex.Message,
                    ChatError.InvalidConnection));

                return;
            }

            OnConnected(EventArgs.Empty);

            StartReceiving();

            _pingTimer = new Timer(Ping, null, 0, _defaultPingDelay);
        }

        public async Task CloseAsync()
        {
            await _client.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "Connection was closed by user.",
                CancellationToken.None)
                .ConfigureAwait(false);

            OnClose(EventArgs.Empty);
        }

        protected void Ping(object sender) =>
            PingAsync(sender).GetAwaiter().GetResult();

        protected async virtual Task OnReconnectedAsync()
        {
            await ConnectAsync().ConfigureAwait(false);

            if (IsConnected && IsAuthorized)
            {
                await Task.Delay(10).ConfigureAwait(false);
                await AuthAsync().ConfigureAwait(false);
            }
        }

        protected async override void OnError(ChatErrorEventArgs e)
        {
            if (e.ErrorCode == ChatError.InvalidConnection)
            {
                await _reconnectionSemph.WaitAsync().ConfigureAwait(false);
                try
                {
                    if (IsConnected)
                        return;

                    Thread.Sleep(_reconnectionDelay);

                    _client.Dispose();
                    _pingTimer?.Dispose();

                    await OnReconnectedAsync().ConfigureAwait(false);
                }
                finally
                {
                    _reconnectionSemph.Release();
                } 
            }

            base.OnError(e);
        }

        protected Task SendAsync(object request) =>
            SendAsync(SerializeRequest(request));

        private async Task SendAsync(byte[] data)
        {
            if (_disposed)
                throw new ObjectDisposedException(
                    "GoodGameChat",
                    "Object was disposed.");

            if (!IsConnected)
            {
                OnError(new ChatErrorEventArgs(
                    "Connection error when sending data.",
                    ChatError.InvalidConnection));

                return;
            }

            var bytesToSend = new ArraySegment<byte>(data);

            try
            {
                await _client.SendAsync(
                    bytesToSend,
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None)
                    .ConfigureAwait(false);
            }
            catch (Exception)
            {
                OnError(new ChatErrorEventArgs(
                    "Connection to the server was unexpectedly interrupted on \"SendAsync\"",
                    ChatError.InvalidConnection));
            }            
        }

        private void StartReceiving()
        {
            new Thread(
                async () =>
                {
                    while (IsConnected)
                    {
                        await ReceiveAsync().ConfigureAwait(false);
                    }
                }).Start();
        }

        private async Task ReceiveAsync()
        {
            if (!IsConnected)
            {
                OnError(new ChatErrorEventArgs(
                    "Connection error when receiving data.",
                    ChatError.InvalidConnection));

                return;
            }

            var data = new List<byte>();
            var bytesReceived = new ArraySegment<byte>(new byte[_receiveBufferSize]);

            WebSocketReceiveResult result = null;

            do
            {
                try
                {
                    result = await _client
                        .ReceiveAsync(bytesReceived, CancellationToken.None)
                        .ConfigureAwait(false);
                }
                catch (Exception)
                {
                    OnError(new ChatErrorEventArgs(
                        "Connection to the server was unexpectedly interrupted on \"ReceiveAsync\"",
                        ChatError.InvalidConnection));

                    return;
                }

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _client.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "The connection has closed after the request was fulfilled.",
                        CancellationToken.None)
                        .ConfigureAwait(false);

                    OnClose(EventArgs.Empty);

                    return;
                }

                for (int i = 0; i < result.Count; i++)
                {
                    data.Add(bytesReceived.Array[i]);
                }
            }
            while (!result.EndOfMessage);
            
            OnReceived(data.ToArray());            
        }

        #region Disposable

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _client.Dispose();
                _client = null;
                _reconnectionSemph.Dispose();
                _pingTimer.Dispose();
                _pingTimer = null;
                _uriString = null;
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
