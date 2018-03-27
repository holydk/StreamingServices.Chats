using System;
using System.Threading.Tasks;

namespace StreamingServices.Models
{
    public interface IChat
    {
        string UserName { get; }

        bool IsJoined { get; }

        bool IsAuthorized { get; }

        Channel Channel { get; }

        event EventHandler<ChatErrorEventArgs> Error;

        event EventHandler Connected;

        event EventHandler Close;

        event EventHandler<ChatMessageEventArgs> Message;

        Task SendMessageAsync(string text);

        Task AuthAsync();
    }
}
