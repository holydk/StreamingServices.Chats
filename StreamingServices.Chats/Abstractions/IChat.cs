using StreamingServices.Chats.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StreamingServices.Chats.Abstractions
{
    public interface IChat : IDisposable
    {
        bool IsConnected { get; }

        bool IsAuthorized { get; }

        string UserName { get; }

        IReadOnlyCollection<Channel> Channels { get; }

        event EventHandler<ChatErrorEventArgs> Error;

        event EventHandler Connected;

        event EventHandler Close;

        event EventHandler<ChatMessageEventArgs> Message;

        Task ConnectAsync(Uri uri, IChatCredential credential);

        Task CloseAsync();

        Task JoinChannel(Channel channel);

        Task UnJoinChannel(Channel channel);

        Task SendMessageAsync(string text);

        Task SendMessageAsync(string text, Channel channel);
    }
}
