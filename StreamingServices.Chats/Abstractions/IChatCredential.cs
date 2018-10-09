using System.Security;

namespace StreamingServices.Chats.Abstractions
{
    public interface IChatCredential
    {
        /// <summary>
        /// User in chat
        /// </summary>
        string User { get; }

        /// <summary>
        /// User token
        /// </summary>
        SecureString Token { get; }
    }
}
