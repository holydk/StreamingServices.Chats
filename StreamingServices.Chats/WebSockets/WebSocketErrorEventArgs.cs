using System;

namespace StreamingServices.Chats.WebSockets
{
    public class WebSocketErrorEventArgs : EventArgs
    {
        public string Message { get; }

        public WebSocketErrorEventArgs(string message)
        {
            Message = message;
        }
    }
}
