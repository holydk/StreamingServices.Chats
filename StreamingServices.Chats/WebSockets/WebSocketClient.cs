using StreamingServices.Chats.Abstractions;
using StreamingServices.Utils.Abstractions;
using StreamingServices.Utils.Logging;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace StreamingServices.Chats.WebSockets
{
    public class WebSocketClient : IWebSocketClient
    {
        #region Private Members

        private ClientWebSocket _client;

        private ILogFactory _logger;

        private int _reconnectionDelay = 5000;

        private int _receiveBufferSize = 1024;

        private CancellationTokenSource _cancelTokenSource;

        private bool _disposedValue = false;

        private readonly SemaphoreSlim _reconnectionSemph = new SemaphoreSlim(1, 1);

        private readonly SemaphoreSlim _sendSemph = new SemaphoreSlim(1, 1);

        #endregion

        #region Public Properties

        /// <summary>
        /// Get the WebSocket state
        /// </summary>
        public WebSocketState State => _client.State;

        /// <summary>
        /// Indicates whether there is a connection to the server socket
        /// </summary>
        public bool IsConnected => State == WebSocketState.Open;

        public Uri Uri { get; private set; }

        #endregion

        #region IWebSocketClient Events

        public event EventHandler Connected;

        public event EventHandler<WebSocketCloseEventArgs> Close;

        public event EventHandler<WebSocketErrorEventArgs> Error;

        public event EventHandler<WebSocketResponseEventArgs> Message;

        #endregion

        #region Constructors

        public WebSocketClient(ILogFactory logger = null)
        {
            _logger = logger;
            _client = new ClientWebSocket();
            _cancelTokenSource = new CancellationTokenSource();
        } 

        #endregion

        public async Task ConnectAsync(Uri uri)
        {
            CheckDisposing();

            if (IsConnected)
                return;

            Uri = uri;
            
            await ConnectAsync().ConfigureAwait(false);
        }

        public async Task ReconnectAsync()
        {
            CheckDisposing();

            await _reconnectionSemph.WaitAsync().ConfigureAwait(false);
            try
            {
                if (IsConnected)
                    return;

                _cancelTokenSource.Cancel();

                await Task.Delay(_reconnectionDelay).ConfigureAwait(false);
               
                _client.Dispose();
                _client = new ClientWebSocket();
                _cancelTokenSource = new CancellationTokenSource();

                await ConnectAsync().ConfigureAwait(false);      
            }
            finally
            {
                _reconnectionSemph.Release();
            }
        }

        public Task CloseAsync()
        {
            return CloseAsync("Closed by user");
        }

        public async Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType)
        {
            CheckDisposing();

            if (!IsConnected || buffer == null || buffer.Array.Length == 0)
                return;

            await _sendSemph.WaitAsync().ConfigureAwait(false);
            try
            {
                await _client
                    .SendAsync(
                        buffer,
                        WebSocketMessageType.Text,
                        true,
                        _cancelTokenSource.Token)
                    .ConfigureAwait(false);
            }
            catch
            {
                _client.Abort();
                LogError("Connection to the server was unexpectedly interrupted on \"SendAsync\"");
            }
            finally
            {
                _sendSemph.Release();
            }
        }

        protected virtual void OnConnected(EventArgs e)
        {
            Connected?.Invoke(this, e);
        }

        protected virtual void OnClose(WebSocketCloseEventArgs e)
        {
            Close?.Invoke(this, e);
        }

        protected virtual void OnError(WebSocketErrorEventArgs e)
        {
            Error?.Invoke(this, e);
        }

        protected virtual void OnMessage(WebSocketResponseEventArgs e)
        {
            Message?.Invoke(this, e);
        }

        private void StartListening()
        {
            new Thread(
                async () =>
                {
                    while (IsConnected)
                    {
                        await ReceiveAsync().ConfigureAwait(false);
                    }
                })
            {
                IsBackground = true
            }
            .Start();
        }

        private async Task ReceiveAsync()
        {
            var data = new List<byte>();
            var bytesReceived = new ArraySegment<byte>(new byte[_receiveBufferSize]);

            WebSocketReceiveResult result = null;

            try
            {
                do
                {
                    result = await _client
                        .ReceiveAsync(bytesReceived, _cancelTokenSource.Token)
                        .ConfigureAwait(false);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await CloseAsync("The connection has closed after the request was fulfilled.");

                        return;
                    }

                    for (int i = 0; i < result.Count; i++)
                    {
                        data.Add(bytesReceived.Array[i]);
                    }
                }
                while (!result.EndOfMessage);
            }
            catch
            {
                _client.Abort();
                LogError("Connection to the server was unexpectedly interrupted on \"ReceiveAsync\"");

                return;
            }

            OnMessage(new WebSocketResponseEventArgs(result.MessageType, data.ToArray()));
        }

        private async Task ConnectAsync()
        {
            try
            {
                await _client
                    .ConnectAsync(Uri, _cancelTokenSource.Token)
                    .ConfigureAwait(false);
            }
            catch
            {
                LogError($"Error on connecting to {Uri}.");

                return;
            }

            OnConnected(EventArgs.Empty);
            StartListening();
        }

        private async Task CloseAsync(string description)
        {
            CheckDisposing();

            if (!IsConnected)
                return;

            try
            {
                await _client
                    .CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        description,
                        _cancelTokenSource.Token)
                    .ConfigureAwait(false);

                OnClose(new WebSocketCloseEventArgs(description));
            }
            catch
            {
                LogError("Exception on closing client.");
            }
        }

        private void LogError(string msg, bool invokeOnError = true)
        {
            _logger?.Log(msg, LogLevel.Error);

            if (invokeOnError)
            {
                OnError(new WebSocketErrorEventArgs(msg));
            }
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _client?.Dispose();
                    _client = null;

                    _cancelTokenSource?.Dispose();
                    _cancelTokenSource = null;

                    _reconnectionSemph?.Dispose();
                    _sendSemph?.Dispose();

                    Uri = null;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void CheckDisposing()
        {
            if (_disposedValue)
            {
                var objName = nameof(WebSocketClient);

                LogError($"{objName} is disposed.", false);

                throw new ObjectDisposedException(objName);
            }
        }

        #endregion
    }
}
