using StreamingServices.Chats.Abstractions;
using StreamingServices.Chats.WebSockets;
using StreamingServices.Utils.Abstractions;
using StreamingServices.Utils.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private HashSet<Channel> _channels;

        #endregion

        #region Public Properties

        public abstract bool IsConnected { get; }

        public bool IsAuthorized { get; protected set; }

        public string UserName { get; protected set; }

        public IReadOnlyCollection<Channel> Channels
        {
            get => _channels.ToList().AsReadOnly();
        }

        #endregion

        #region Events

        public event EventHandler<ChatErrorEventArgs> Error;

        public event EventHandler Connected;

        public event EventHandler Close;

        public event EventHandler<ChatMessageEventArgs> Message;

        #endregion

        #region Constructors

        protected Chat(ILogFactory logger = null)
        {
            _logger = logger;
            _channels = new HashSet<Channel>(new ChannelEqualityComparer());
        }

        #endregion

        public Task ConnectAsync(Uri uri, IChatCredential credential)
        {
            CheckDisposing();

            if (IsConnected)
                return Task.CompletedTask;

            _credential = credential;

            return ConnectAsync(uri);
        }

        public Task CloseAsync()
        {
            CheckDisposing();

            if (!IsConnected)
                return Task.CompletedTask;

            _channels.Clear();
            _channels = null;
            UserName = null;
            IsAuthorized = false;

            return InternalCloseAsync();
        }

        protected Task SendAsync(string msg)
        {
            CheckDisposing();

            if (string.IsNullOrWhiteSpace(msg))
                return Task.CompletedTask;

            return InternalSendAsync(msg);
        }

        public async Task SendMessageAsync(string text, Channel channel)
        {
            if (string.IsNullOrWhiteSpace(text) || !IsAuthenticated())
                return;

            if (channel == null)
                throw new ArgumentNullException(nameof(channel));

            if (!_channels.Contains(channel))
            {
                OnError(new ChatErrorEventArgs(
                    $"Error on send message: Channel - {channel} not found.", 
                    ChatError.NotFoundChannel));
            }
            else
            {
                await InternalSendMessageAsync(text, channel);
            }
        }

        public async Task SendMessageAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || !IsAuthenticated())
                return;            

            if (!_channels.Any())
            {
                OnError(new ChatErrorEventArgs(
                    "Error on send message: Channels list is empty.",
                    ChatError.EmptyChannelsList));
            }
            else
            {
                foreach (var channel in _channels)
                {
                    await InternalSendMessageAsync(text, channel);
                }
            }
        }

        public async Task JoinChannel(Channel channel)
        {
            if (channel == null)            
                throw new ArgumentNullException(nameof(channel));

            if (!IsAuthenticated())
                return;

            if (_channels.Contains(channel))
            {
                OnError(new ChatErrorEventArgs(
                    $"The attempt to connect to the same channel - {channel} in chat.",
                    ChatError.ConnectionToSameChannel));
            }
            else
            {
                await InternalJoinChannel(channel);
            }
        }

        public async Task UnJoinChannel(Channel channel)
        {
            if (channel == null)
                throw new ArgumentNullException(nameof(channel));

            if (!IsAuthenticated())
                return;

            if (!_channels.Contains(channel))
            {
                OnError(new ChatErrorEventArgs(
                    $"The attempt to unjoin not added channel - {channel}.",
                    ChatError.NotFoundChannel));
            }
            else
            {
                await InternalUnJoinChannel(channel);
            }
        }

        protected abstract Task ConnectAsync(Uri uri);

        protected abstract Task AuthorizeAsync(IChatCredential credential);

        protected abstract Task InternalCloseAsync();

        protected abstract Task InternalSendAsync(string msg);

        protected abstract Task InternalSendMessageAsync(string text, Channel channel);

        protected abstract Task InternalJoinChannel(Channel channel);

        protected abstract Task InternalUnJoinChannel(Channel channel);

        protected virtual void OnError(ChatErrorEventArgs e)
        {
            if (e.ErrorCode == ChatError.InvalidConnection)           
                IsAuthorized = false;                   

            Error?.Invoke(this, e);
        }
            
        protected virtual void OnClose(EventArgs e)
        {
            Close?.Invoke(this, e);
        }

        protected virtual void OnConnected(EventArgs e)
        {
            Task.Factory.StartNew(async () =>
            {
                await AuthorizeAsync(_credential).ConfigureAwait(false);
                
                if (_channels.Any())
                {
                    // Waiting for response from server
                    // with authorization data
                    await Task.Delay(2000);

                    foreach (var channel in _channels)
                    {
                        await InternalJoinChannel(channel);
                    }
                }
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
                OnError(new ChatErrorEventArgs(msg, type.Value));            
        }

        protected void AddChannel(Channel channel)
        {
            if (channel == null)
                throw new ArgumentNullException(nameof(channel));

            _channels.Add(channel);
        }

        protected void RemoveChannel(Channel channel)
        {
            if (channel == null)
                throw new ArgumentNullException(nameof(channel));

            _channels.Remove(channel);
        }

        protected bool IsAuthenticated()
        {
            if (!IsConnected)
            {
                OnError(new ChatErrorEventArgs(
                    "You not connected to chat.",
                    ChatError.InvalidConnection));

                return false;
            }

            if (!IsAuthorized)
            {
                OnError(new ChatErrorEventArgs(
                    "You not authorized in chat.",
                    ChatError.NotAuthorized));

                return false;
            }

            return true;
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
            _channels.Clear();
            _channels = null;
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
}
