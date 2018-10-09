using System;

namespace StreamingServices.Chats.Models
{
    public class ChatMessageEventArgs : EventArgs
    {
        public string Text { get; }
        public string UserName { get; }
        public string UserColor { get; }
        public Channel Channel { get; }

        public ChatMessageEventArgs(string text, string userName, string userColor, Channel channel)
        {
            Text = text;
            UserName = userName;
            UserColor = userColor;
            Channel = channel;
        }
    }
}
