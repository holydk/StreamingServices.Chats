using System;

namespace StreamingServices.Chats.WebSockets
{
    public class WebSocketCloseEventArgs : EventArgs
    {
        public string Description { get; set; }

        public WebSocketCloseEventArgs(string description)
        {
            Description = description;
        }
    }
}
