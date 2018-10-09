using System;
using System.Net.WebSockets;

namespace StreamingServices.Chats.WebSockets
{
    public class WebSocketResponseEventArgs : EventArgs
    {
        public WebSocketMessageType MessageType { get; }

        public byte[] Data { get; }

        public WebSocketResponseEventArgs(WebSocketMessageType type, byte[] data)
        {
            MessageType = type;
            Data = data;
        }
    }
}
