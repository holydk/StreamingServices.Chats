using StreamingServices.Chats.Models;
using System;
using System.Threading.Tasks;

namespace StreamingServices.Chats.Abstractions
{
    public interface IChat : IDisposable
    {
        bool IsConnected { get; }

        bool IsJoined { get; }

        bool IsAuthorized { get; }

        string UserName { get; }

        IChannel Channel { get; }

        event EventHandler<ChatErrorEventArgs> Error;

        event EventHandler Connected;

        event EventHandler Close;

        event EventHandler<ChatMessageEventArgs> Message;

        Task ConnectAsync(Uri uri, IChatCredential credential);

        Task CloseAsync();

        Task JoinChannel(IChannel channel);

        Task UnJoinChannel(IChannel channel);

        Task SendMessageAsync(string text);
    }
}
