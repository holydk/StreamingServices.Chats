using System;

namespace StreamingServices.Models
{
    public class ChatErrorEventArgs : EventArgs
    {
        public string Message { get; }
        public ChatError ErrorCode { get; }

        public ChatErrorEventArgs(string msg, ChatError code)
        {
            Message = msg;
            ErrorCode = code;
        }
    }
}
