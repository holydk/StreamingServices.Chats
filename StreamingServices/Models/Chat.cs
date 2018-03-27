using System;
using System.Threading.Tasks;

namespace StreamingServices.Models
{
    /// <summary>
    /// Base class for Chat
    /// </summary>
    /// <typeparam name="TCommandType"></typeparam>
    public abstract class Chat<TCommandType> : ClientWebService<TCommandType>, IChat
        where TCommandType : struct, IConvertible
    {
        public Chat(string uriString)
            : base(uriString)
        { }

        /// <summary>
        /// User name in chat
        /// </summary>
        public string UserName => _userName;
        protected string _userName;

        protected string _token;

        /// <summary>
        /// Current Channel
        /// </summary>
        public Channel Channel => _channel;
        protected Channel _channel;

        public bool IsAuthorized => _isAuthorized;
        protected volatile bool _isAuthorized = false;

        /// <summary>
        /// Indicates whether there is a connection to the channel
        /// </summary>
        public bool IsJoined => _channel != null;

        public event EventHandler<ChatErrorEventArgs> Error;

        public event EventHandler Connected;

        public event EventHandler Close;

        public event EventHandler<ChatMessageEventArgs> Message;

        /// <summary>
        /// Connect to chat server
        /// </summary>
        /// <returns></returns>
        public abstract Task ConnectAsync();

        /// <summary>
        /// Authorize into chat
        /// </summary>
        /// <returns></returns>
        public abstract Task AuthAsync();

        /// <summary>
        /// Send message in chat
        /// </summary>
        /// <param name="text">text</param>
        /// <returns></returns>
        public abstract Task SendMessageAsync(string text);

        protected virtual void OnError(ChatErrorEventArgs e) => 
            Error?.Invoke(this, e);

        protected virtual void OnClose(EventArgs e) =>
            Close?.Invoke(this, e);

        protected virtual void OnConnected(EventArgs e) =>
            Connected?.Invoke(this, e);

        protected virtual void OnMessage(ChatMessageEventArgs e) =>
            Message?.Invoke(this, e);
    }
}
