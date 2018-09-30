using System;
using System.Drawing;

namespace StreamingServices.Chats.Models
{
    public class ChatMessageEventArgs : EventArgs
    {
        public string Text { get; }
        public string UserName { get; }
        public Color UserColor { get; }

        public ChatMessageEventArgs(string text, string userName, Color userColor)
        {
            Text = text;
            UserName = userName;
            UserColor = userColor;
        }
    }
}
