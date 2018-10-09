using StreamingServices.Chats.WebSockets;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace StreamingServices.Chats.Abstractions
{
    public interface IWebSocketClient : IDisposable
    {
        event EventHandler Connected;
        event EventHandler<WebSocketCloseEventArgs> Close;
        event EventHandler<WebSocketErrorEventArgs> Error;
        event EventHandler<WebSocketResponseEventArgs> Message;

        WebSocketState State { get; }
        bool IsConnected { get; }
        Uri Uri { get; }

        Task ConnectAsync(Uri uri);
        Task ReconnectAsync();
        Task CloseAsync();
        Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType);
    }
}
