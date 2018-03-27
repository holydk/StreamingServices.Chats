using System;
using System.Threading.Tasks;

namespace StreamingServices.Models
{
    public interface IChat
    {
        bool IsJoined { get; }

        bool IsAuthorized { get; }

        string UserName { get; }

        Channel Channel { get; }

        event EventHandler<ChatErrorEventArgs> Error;

        event EventHandler Connected;

        event EventHandler Close;

        event EventHandler<ChatMessageEventArgs> Message;

        Task ConnectAsync();

        Task AuthAsync();

        Task SendMessageAsync(string text);
    }
}
